using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business.FetchStrategies;
using static Enterprise.Integration.Customs;

namespace Enterprise.Customs.IL.Business
{
	public class CusEntryInstruction : AutoILCusEntryInstruction, ICusSupportingInfoTypeSupporter
	{
		public CusEntryInstruction(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoILCusEntryInstruction.Schema
		{
			public const string CEI_FormattedProcedure = "CEI_FormattedProcedure";
			public const string FromWarehouseOrgPK = "FromWarehouseOrgPK";
			public const string ToWarehouseOrgPK = "ToWarehouseOrgPK";

			public const int CEI_FormattedProcedureMaxLength = 7;
		}

		#region Properties

		[MaxLength(Schema.CEI_FormattedProcedureMaxLength)]
		[ResourceStringData("172210C2-E0F4-4C6F-BF43-EBEA06098878", Caption = "Procedure")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ProcedureCodeList))]
		public ZString CEI_FormattedProcedure
		{
			get => CEI_Procedure + CEI_Style;
			set
			{
				if (CEI_FormattedProcedure != value)
				{
					CheckMaximumLength(CEI_FormattedProcedureInfo, value);
					CEI_Procedure = value.Left(4);
					CEI_Style = value.SubstringSafe(4, 3);
					CEI_FormattedProcedureInfo.RefreshBinding();

					if (!IsValidationSuspended)
					{
						Validation.ValidateCEI_FormattedProcedure();
					}
				}
			}
		}

		public ZPropertyInfo CEI_FormattedProcedureInfo
		{
			[DebuggerStepThrough]
			get
			{
				return GetZPropertyInfo(nameof(CEI_FormattedProcedure));
			}
		}

		[ResourceStringData("4CF4084C-E20B-460F-8511-BC3D957F448D", Caption = "Autonomy Region")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.AutonomyRegionTypeList))]
		public override ZString CEI_AutonomyRegionType { get => base.CEI_AutonomyRegionType; set => base.CEI_AutonomyRegionType = value; }

		[ResourceStringData("E132E502-1816-4A4B-B2F4-28874A210AA1", Caption = "To Warehouse Address")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.ToWarehouseBondedWarehouseAddressList))]
		public override ZGuid CEI_OA_Warehouse2 { get => base.CEI_OA_Warehouse2; set => base.CEI_OA_Warehouse2 = value; }

		[ResourceStringData("1AC208A8-8802-4E6B-A343-B9025E470602", Caption = "To Warehouse")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedWarehouseCollection))]
		public ZGuid ToWarehouseOrgPK
		{
			get => CEI_OA_Warehouse2_ZAddress.OrgPK;
			set => CEI_OA_Warehouse2_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo ToWarehouseOrgPKInfo
		{
			get => GetWrappedZPropertyInfo(Schema.ToWarehouseOrgPK, x => CEI_OA_Warehouse2_ZAddress.OrgPKInfo);
		}

		[ResourceStringData("26B28059-9125-4AF0-BD75-AB3C329AF6D4", Caption = "From Warehouse Address")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.FromWarehouseBondedWarehouseAddressList))]
		public override ZGuid CEI_OA_Warehouse { get => base.CEI_OA_Warehouse; set => base.CEI_OA_Warehouse = value; }

		[ResourceStringData("A3943BDF-FD8E-4FA2-B6BD-F1D32B72055D", Caption = "From Warehouse")]
		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.BondedWarehouseCollection))]
		public ZGuid FromWarehouseOrgPK
		{
			get => CEI_OA_Warehouse_ZAddress.OrgPK;
			set => CEI_OA_Warehouse_ZAddress.OrgPK = value;
		}

		public ZPropertyInfo FromWarehouseOrgPKInfo
		{
			get => GetWrappedZPropertyInfo(Schema.FromWarehouseOrgPK, x => CEI_OA_Warehouse_ZAddress.OrgPKInfo);
		}

		[ResourceStringData("E7DA9F41-CA20-4A95-A392-61DCDC55A8CE", Caption = "Taxation Date")]
		public override ZDateTime CEI_DateForDuty { get => base.CEI_DateForDuty; set => base.CEI_DateForDuty = value; }

		[ResourceStringData("A4594E6C-A654-40FB-90DC-5A4A4B423F27", Caption = "Customs Quantity")]
		public override ZInt CEI_NumberOfPackages { get => base.CEI_NumberOfPackages; set => base.CEI_NumberOfPackages = value; }

		[List(nameof(Lookups) + "." + nameof(CusEntryInstructionLookups.PackageUQList))]
		[ResourceStringData("6772F1B0-01B8-4CB9-BD54-88B811A1E148", Caption = "Package Type", ShortCaption = "Pack Type")]
		public override ZString CEI_CustomsPackType { get => base.CEI_CustomsPackType; set => base.CEI_CustomsPackType = value; }

		#endregion Properties

		#region PreviousDocuments

		[ChildEditable(true)]
		public PreviousDocumentCollection PreviousDocuments
		{
			get
			{
				if (fPreviousDocuments == null)
				{
					fPreviousDocuments = new PreviousDocumentCollection(this);
					fPreviousDocuments.Load();
					RegisterEditableChildObject(fPreviousDocuments);
				}
				return fPreviousDocuments;
			}
		}
		PreviousDocumentCollection fPreviousDocuments;

		#endregion PreviousDocuments

		public new CusEntryInstructionValidation Validation => (CusEntryInstructionValidation)base.Validation;
		protected override Customs.Business.CusEntryInstructionValidation GetNewValidation() => new CusEntryInstructionValidation(this);

		public new CusEntryInstructionLookups Lookups => (CusEntryInstructionLookups)base.Lookups;
		protected override Customs.Business.CusEntryInstructionLookups GetNewLookups() => new CusEntryInstructionLookups(this);

		#region ICusSupportingInfoTypeSupporter Members
		IDictionary<ZString, Type> ICusSupportingInfoTypeSupporter.GetCusSupportingInfoTypes()
		{
			return new Dictionary<ZString, Type>
			{
				{ Common.IL.CusSupportingInfoTypeList.Codes.PreviousDocument, typeof(PreviousDocument) },
			};
		}

		IEnumerable<IBusinessObjectFetchStrategy> IAdditionalBusinessObjectFetchStrategyProvider.GetFetchStrategies()
		{
			yield return new CusSupportingInfoTypeSupporterFetchStrategy(this);
		}
		#endregion
	}
}
