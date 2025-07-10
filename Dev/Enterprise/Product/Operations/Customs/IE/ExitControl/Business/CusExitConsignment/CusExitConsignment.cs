using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.IE.ExitControl.Business
{
	public class CusExitConsignment : EU.ExitControl.Business.CusExitConsignment
		, Integration.Customs.IEExitControl.ICusExitConsignment
		, EU.Business.ICusAuthorizationUsageMaster
	{
		public CusExitConsignment(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		[ChildEditable]
		public EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment> CusAuthorizationUsages
		{
			get
			{
				if (cusAuthorizationUsages == null)
				{
					cusAuthorizationUsages = GetNewCusAuthorizationUsages();
					cusAuthorizationUsages.Load();
					cusAuthorizationUsages.IsManagedForDataRefresh = true;
					RegisterEditableChildObject(cusAuthorizationUsages);
				}
				return cusAuthorizationUsages;
			}
		}
		EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment> cusAuthorizationUsages;

		protected virtual EU.Business.ICusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment> GetNewCusAuthorizationUsages() => new CusAuthorizationUsageCollection<CusAuthorizationUsage, CusExitConsignment>(this, Factory);

		public override void Delete()
		{
			if (!IsDeleted)
			{
				CusAuthorizationUsages.RemoveAndDeleteAll();
			}
			base.Delete();
		}

		public SchemaGuidColumn FKSchemaColumnInDependent => CusAuthorizationUsageSchema.AGC_ParentID;

		public new CusExitConsignmentValidation Validation => (CusExitConsignmentValidation)base.Validation;
		protected override ExitControlBase.Business.CusExitConsignmentValidation GetNewValidation() => new CusExitConsignmentValidation(this);

		public new ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem> CusExitConsignmentItems => (ExitControlBase.Business.ICusExitConsignmentItemCollection<CusExitConsignmentItem>)base.CusExitConsignmentItems;

		protected override ExitControlBase.Business.ICusExitConsignmentItemCollection<ExitControlBase.Business.CusExitConsignmentItem> CreateNewCusExitConsignmentItemCollection() => new ExitControlBase.Business.CusExitConsignmentItemCollection<CusExitConsignmentItem>(this);

		protected override bool IsMovementReferenceReadOnly => Header.CusExitReports?.Any(x => x.CER_CXC_Consignment == PK && (!x.CER_Status.IsEmpty || x.CER_MessageStatus == LogicalStatusList.Codes.Sent)) ?? false;

		public void ImportGoodsItemData(JobDeclaration declaration)
		{
			var entry = declaration?.CustomsEntryHeaders.FirstOrDefault(x => x.MovementReferenceNumber == CXC_MovementReference);
			if (entry != null)
			{
				var exitContainers = Header.CusExitContainers.GroupBy(x => x.CXN_IsEquipment).ToDictionary(x => x.Key, y => y.Select(x => x.CXN_ContainerNumber).ToHashSet());

				var mapping = entry.GetContainerOrEquipmentToEntryLineMapping();
				var containers = mapping.containers.Select(x => x.Key);
				ImportContainerData(containers, exitContainers.GetOrAdd(ZBool.False, () => new HashSet<ZString>()));

				var equipments = mapping.equipments.Select(x => x.Key);
				ImportEquipmentData(equipments, exitContainers.GetOrAdd(ZBool.True, () => new HashSet<ZString>()));

				ImportEntryLineData(entry);
			}
		}

		void ImportContainerData(IEnumerable<Customs.Business.BaseCusContainer> containers, HashSet<ZString> exitContainers)
		{
			foreach (CusContainer container in containers)
			{
				var containerNumber = container.CO_ContainerNumber.Left(CusExitContainer.Schema.CXN_ContainerNumberMaxLength);
				if (!exitContainers.Contains(containerNumber))
				{
					var exitContainer = (CusExitContainer)Header.CusExitContainers.AddNew();
					exitContainer.CXN_ContainerNumber = containerNumber;
					exitContainer.CXN_IsEquipment = false;
					var sealNumberMaxLength = EU.Business.Declaration.CusSeal.Schema.BK_SealNumberMaxLength;
					var seals = CollectIfNotEmpty(container.CO_Seal.Left(sealNumberMaxLength), container.CO_SecondSeal.Left(sealNumberMaxLength), container.AdditionalSeals.OrderBy(x => x.BK_SequenceNumber).Select(x => x.BK_SealNumber));
					ImportSealData(exitContainer, seals);
				}
			}
		}

		void ImportSealData(EU.ExitControl.Business.CusExitContainer exitContainer, List<ZString> seals)
		{
			seals.ForEach(additionalSeal => { exitContainer.AllSealNumbers.AddNew().BK_SealNumber = additionalSeal; });
		}

		List<ZString> CollectIfNotEmpty(ZString value1, ZString value2, IEnumerable<ZString> additionalValues)
		{
			var result = new List<ZString>();
			if (!value1.IsEmpty)
			{
				result.Add(value1);
			}
			if (!value2.IsEmpty)
			{
				result.Add(value2);
			}
			result.AddRange(additionalValues.Where(x => !x.IsEmpty));
			return result;
		}

		void ImportEquipmentData(IEnumerable<Customs.Business.CusEquipment> equipments, HashSet<ZString> exitEquipments)
		{
			foreach (EU.Business.Declaration.CusEquipment equipment in equipments)
			{
				var identificationNumber = equipment.CEQ_IdentificationNumber.Left(CusExitContainer.Schema.CXN_ContainerNumberMaxLength);
				if (!exitEquipments.Contains(identificationNumber))
				{
					var exitEquipment = Header.CusExitContainers.AddNew();
					exitEquipment.CXN_ContainerNumber = identificationNumber;
					exitEquipment.CXN_IsEquipment = true;
					var seals = CollectIfNotEmpty(ZString.Empty, ZString.Empty, equipment.Seals.OrderBy(x => x.BK_SequenceNumber).Select(x => x.BK_SealNumber));
					ImportSealData(exitEquipment, seals);
				}
			}
		}

		void ImportEntryLineData(CusEntryHeader entry)
		{
			var lineNumbers = CusExitConsignmentItems.Select(x => x.CCI_LineNumber).ToHashSet();
			foreach (var entryLine in entry.MergedLines)
			{
				var lineNumber = entryLine.CL_LineNumber;
				if (!lineNumbers.Contains(lineNumber))
				{
					var consignmentItem = CusExitConsignmentItems.AddNew();
					consignmentItem.CCI_LineNumber = lineNumber;
					consignmentItem.CCI_GrossMass = entryLine.GrossWeight.InKilogramsSafe;
					consignmentItem.CCI_NetMass = entryLine.EffectiveCustomsWeight.InKilogramsSafe;
					consignmentItem.CCI_UniqueConsignmentReference = entryLine.RandomLine.InvoiceHeader?.JZ_UCR ?? ZString.Empty;

					foreach (var entryLinePivot in entryLine.PackagingDetails)
					{
						var entryLinePackage = entryLinePivot.Package;
						if (entryLinePackage != null)
						{
							var pivot = consignmentItem.CusExitConsignmentPackagePivots.AddNew();
							pivot.CNP_CXN_Container = Header.CusExitContainers.FirstOrDefault(x => x.CXN_ContainerNumber == entryLinePackage.CW_ContainerNoOrEquipmentNo)?.PK ?? ZGuid.Empty;
							var package = pivot.Package;
							package.CXP_Quantity = entryLinePivot.CHC_Quantity.ToZInt();
							package.CXP_PackageType = entryLinePackage.CW_PackType;
							package.CXP_MarksAndNumbers = entryLinePackage.CW_MarksAndNos.Truncate(CusExitConsignmentPackage.Schema.CXP_MarksAndNumbersMaxLength);
						}
					}
				}
			}
		}
	}
}
