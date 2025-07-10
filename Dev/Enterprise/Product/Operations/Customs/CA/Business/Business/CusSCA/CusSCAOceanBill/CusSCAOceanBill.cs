using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Customs;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CA.Business
{
	public class CusSCAOceanBill : BaseCusSCAOceanBill,
	IMessageManageableBizObj,
	IWorkflowProvider,
	IWorkflowTriggerEventSource,
	Integration.Customs.CA.ICusSCAOceanBill
	{
		public CusSCAOceanBill(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}
		public static new readonly TypeDecider TypeDecider = new CusSCAOceanBillTypeDecider();

		public static CusSCAOceanBill GetNewOceanBill(ForwardingConsol consol)
		{
			var result = consol.Factory.New<CusSCAOceanBill>();
			result.CB_ParentId = consol.PK;
			result.CB_ParentTableCode = JobConsolSchema.Constants.Prefix;
			if (consol.IsAir)
			{
				result.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
			}
			else if (consol.IsRail)
			{
				result.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail;
			}
			else if (consol.IsRoad)
			{
				result.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad;
			}
			else
			{
				result.CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea;
			}
			return result;
		}

		public bool IsInAStatusAmendmentSendable
		{
			get { return false; }
		}

		public ContinueWithDetection ProcessBeforeDetectingAmendmentAndContinue()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		public IMessageManager GetMessageManagerForAmendmentDetection()
		{
			throw new NotSupportedException("The method is not supported.");
		}

		public static ZString[] ApplicationCodes
		{
			get
			{
				return new ZString[] { Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir,
Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad, Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail };
			}
		}

		public override ZString[] ApplicationCodesForBase
		{
			get { return ApplicationCodes; }
		}

		public override ZString CB_ApplicationCode
		{
			get => base.CB_ApplicationCode;
			set
			{
				var oldValue = CB_ApplicationCode;
				base.CB_ApplicationCode = value;
				if (!IsCopying && oldValue != CB_ApplicationCode)
				{
					HouseBills.MarkAsNeedingValidation();
					HouseBills.ForEach(x => x.PackLines.MarkAsNeedingValidation());
				}
			}
		}

		[ReadOnly(true)]
		public override ZString CB_OceanBill
		{
			get { return base.CB_OceanBill; }
		}

		public bool IsAttachedToConsol
		{
			get { return Consol != null; }
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			CB_ApplicationCode = Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir;
		}

		public ZBool IsAir
		{
			get { return CB_ApplicationCode == Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIAir; }
		}

		public ZBool IsSea
		{
			get { return CB_ApplicationCode == Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACISea; }
		}

		public ZBool IsRoad
		{
			get { return CB_ApplicationCode == Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRoad; }
		}

		public ZBool IsRail
		{
			get { return CB_ApplicationCode == Core.Constants.Customs.CusSCAOceanBillApplicationCodes.CanadaACIRail; }
		}

		public bool ShouldSyncroniseWithConsol
		{
			get
			{
				if (shouldSyncroniseWithConsol == null)
				{
					shouldSyncroniseWithConsol = new CachedProperty<bool>(Factory, delegate
					{
						bool result = Consol != null;
						if (result)
						{
							foreach (var house in HouseBills)
							{
								result = house.ShouldSynchronizeWithShipment;
								if (!result)
								{
									break;
								}
							}
						}
						return result;
					});
				}
				return shouldSyncroniseWithConsol.Value;
			}
		}
		CachedProperty<bool> shouldSyncroniseWithConsol;

		public CusSCAOceanBillSynchroniser ConsolSynchroniser
		{
			get
			{
				if (fConsolSynchroniser == null)
				{
					if (Consol == null)
					{
						throw new NotSupportedException("You can't synchronise with a Consol when you don't have a Consol.");
					}

					fConsolSynchroniser = new CusSCAOceanBillSynchroniser(this, Consol);
				}
				return fConsolSynchroniser;
			}
		}
		CusSCAOceanBillSynchroniser fConsolSynchroniser;

		public void EnableAndSynchronise(bool forceSynch = false)
		{
			if (Consol != null)
			{
				using (GetValidationSuspender())
				{
					ConsolSynchroniser.SetEnabled(ShouldSyncroniseWithConsol, ConsolSynchroniser.DetectEnabled);
					if (forceSynch)
					{
						ConsolSynchroniser.Synchronise(forceSynch);
					}
					else
					{
						ConsolSynchroniser.Synchronise();
					}
				}
			}
		}

		[BusinessObjectTestExclude()]
		[CargoWise.ComponentModel.MaxLength(25)]
		public ZString OriginalCCN
		{
			get { return PCNCusEntryNum.CE_EntryNum; }
			set
			{
				PCNCusEntryNum.CE_EntryNum = value.Left(OriginalCCNInfo.MaxLength);
				MarkAsNeedingValidation();
				OriginalCCNInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo OriginalCCNInfo
		{
			get { return GetZPropertyInfo(nameof(OriginalCCN)); }
		}

		public CusEntryNumber PCNCusEntryNum
		{
			get
			{
				if (pCNCusEntryNum == null)
				{
					ZQuery filter = new ZQuery(CusEntryNumSchema.CE_ParentID, this.PK);
					filter.AddToFilter(CusEntryNumSchema.CE_EntryType, CanadaAdditionalReferenceNumberTypes.Codes.PCN);
					//Filter.AddToFilter(CusEntryNumSchema.CE_RN_NKCountryCode, Core.Constants.CountryCodes.Canada);// do not do this, must pickup data from non-CA branches
					pCNCusEntryNum = Factory.LoadTop1<CusEntryNumber>(filter);
					if (pCNCusEntryNum == null)
					{
						pCNCusEntryNum = CusEntryNumber.New(this, CanadaAdditionalReferenceNumberTypes.Codes.PCN, Core.Constants.CountryCodes.Canada);
					}
					RegisterEditableChildObject(pCNCusEntryNum);
					pCNCusEntryNum.CE_ParentIDInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					pCNCusEntryNum.CE_ParentTableInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					pCNCusEntryNum.CE_RN_NKCountryCodeInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					pCNCusEntryNum.CE_CategoryInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					pCNCusEntryNum.CE_EntryNumInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
					pCNCusEntryNum.CE_EntryTypeInfo.ValueChanged += delegate
					{ MarkAsNeedingValidation(); };
				}
				return pCNCusEntryNum;
			}
		}
		CusEntryNumber pCNCusEntryNum;

		public void RegisterChildEditableForConsolUse()
		{
			if (!IsRegisteredEditableChildObject(HouseBills))
			{
				RegisterEditableChildObject(houseBills);
			}
		}

		[ChildEditableTestExclude()]
		public CusSCAHouseCollectionForOceanBill HouseBills
		{
			get
			{
				if (houseBills == null)
				{
					houseBills = new CusSCAHouseCollectionForOceanBill(this);
					if (!IsAttachedToConsol)
					{
						RegisterEditableChildObject(houseBills);
					}
				}
				return houseBills;
			}
		}
		CusSCAHouseCollectionForOceanBill houseBills;

		[ChildEditable()]
		public CusSCAContainerCollectionForOceanBill Containers
		{
			get
			{
				if (containers == null)
				{
					containers = new CusSCAContainerCollectionForOceanBill(this);
					if (!IsAttachedToConsol)
					{
						RegisterEditableChildObject(containers);
					}
				}
				return containers;
			}
		}
		CusSCAContainerCollectionForOceanBill containers;

		protected override void OnFactorySavingBeforeTransactionCore()
		{
			base.OnFactorySavingBeforeTransactionCore();
			new ProcessTask.Loader(Factory).CreateTasksAndMilestonesFromTemplateIfRequired(this);
		}

		public override void Delete()
		{
			Containers.DeleteAll();
			HouseBills.DeleteAll();
			WorkflowItems.RemoveAndDeleteAll();
			base.Delete();
		}

		public CusSCAContainer FindContainerByNumber(ZString containerNumber)
		{
			CusSCAContainer result = null;
			foreach (CusSCAContainer container in Containers)
			{
				if (!container.IsDeleted && container.CN_ContainerNumber == containerNumber)
				{
					result = container;
					break;
				}
			}
			return result;
		}

		public new CusSCAOceanBillValidation Validation
		{
			get { return (CusSCAOceanBillValidation)base.Validation; }
		}

		protected override DocManagerInfo GetDocManagerInfo() => new DocManagerInfo(this, Core.Constants.DocManagerCodes.SCAOceanBill);

		protected override Customs.Business.CusSCAOceanBillValidation GetNewValidation()
		{
			return new CusSCAOceanBillValidation(this);
		}

		protected override bool IsCrossCompanyRecord
		{
			get { return true; }
		}

		#region IWorkflowProvider

		protected override bool SupportsWorkflowCore => true;

		protected override ZString WorkflowTypeCore
		{
			get { return WorkflowDescriptors.CusSCAOceanBillDescriptorCode; }
		}

		protected override ProcessTaskCollection GetNewCusSCAOceanBillProcessTaskCollection()
		{
			return new ProcessTaskCollection<CusSCAOceanBillProcessTask, CusSCAOceanBill>(this);
		}

		#endregion

		#region IWorkflowTriggerEventSource

		internal GlbCompany Company
		{
			get
			{
				return Branch?.Company ?? Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			}
		}

		IGlbCompany IWorkflowTriggerEventSource.JobHeaderCompany
		{
			get { return Company; }
		}

		IReadOnlyList<IWorkflowProviderCore> IWorkflowTriggerEventSource.ParentWorkflowProviders
		{
			get
			{
				return IsAttachedToConsol ? new IWorkflowProviderCore[] { Consol } : Array.Empty<IWorkflowProviderCore>();
			}
		}
		#endregion // IWorkflowTriggerEventSource
	}
}
