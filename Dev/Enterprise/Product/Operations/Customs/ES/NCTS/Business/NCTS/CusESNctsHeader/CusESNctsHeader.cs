using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.Customs.ES.NCTS.Business
{
	[DependentBusinessObject(typeof(NctsHeader), "PK")]
	public class CusESNctsHeader : AutoCusESNctsHeader
	{
		public CusESNctsHeader(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public new class Schema : AutoCusESNctsHeader.Schema
		{
			public const int TIRCarnetPageMaxLength = 4;
		}

		public NctsHeader Header => Factory.Load<NctsHeader>(CEN_BH);

		[List(nameof(Lookups) + "." + nameof(CusESNctsHeaderLookups.SummaryTypeList))]
		[ResourceStringData("ES.CusESNctsHeader.CEN_SummaryType", Caption = "Summary Type", MediumCaption = "Summary Type", ShortCaption = "Summary Type")]
		public override ZString CEN_SummaryType { get => base.CEN_SummaryType; set => base.CEN_SummaryType = value; }

		[List(nameof(Header) + "." + nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.NatSimplificationIndicator))]
		public override ZString CEN_NationalSimplificatorInd
		{
			get => base.CEN_NationalSimplificatorInd;
			set
			{
				var oldValue = CEN_NationalSimplificatorInd;
				base.CEN_NationalSimplificatorInd = value;
				if (!IsCopying && oldValue != CEN_NationalSimplificatorInd)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CEN_TNNDocumentType
		{
			get => base.CEN_TNNDocumentType;
			set
			{
				var oldValue = CEN_TNNDocumentType;
				base.CEN_TNNDocumentType = value;
				if (!IsCopying && oldValue != CEN_TNNDocumentType)
				{
					Header.MarkAsNeedingValidation();
					if (Header.IsPhase5)
					{
						((NctsHeaderPhase5Validation)Header.Validation).ValidateMovementReferenceNumber();
					}
				}
			}
		}

		public override ZGuid CEN_OA_DeclarantAddress
		{
			get => base.CEN_OA_DeclarantAddress;
			set
			{
				var oldValue = CEN_OA_DeclarantAddress;
				base.CEN_OA_DeclarantAddress = value;
				if (!IsCopying && oldValue != CEN_OA_DeclarantAddress)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CEN_DeclarantEmailAddress
		{
			get => base.CEN_DeclarantEmailAddress;
			set
			{
				var oldValue = CEN_DeclarantEmailAddress;
				base.CEN_DeclarantEmailAddress = value;
				if (!IsCopying && oldValue != CEN_DeclarantEmailAddress)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		[LightValidationTestExempt]
		[ResourceStringData("ES.CusESNctsHeader.CEN_PreviousSummaryDeclaration", Caption = "Previous Summary", MediumCaption = "Previous Summary", ShortCaption = "Prev. Summary")]
		public override ZString CEN_PreviousSummaryDeclaration
		{
			get => base.CEN_PreviousSummaryDeclaration;
			set
			{
				var oldValue = CEN_PreviousSummaryDeclaration;
				base.CEN_PreviousSummaryDeclaration = value;
				if (!IsCopying && oldValue != CEN_PreviousSummaryDeclaration)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("ES.CusESNctsHeader.CEN_TIRArrival", Caption = "TIR Arrival")]
		public override ZBool CEN_TIRArrival { get => base.CEN_TIRArrival; set => base.CEN_TIRArrival = value; }

		public ZBool IsShowTIRArrivalDetails => CEN_TIRArrival;

		[ResourceStringData("ES.CusESNctsHeader.CEN_TIRPartialUnloading", Caption = "TIR Partial Unloading")]
		public override ZBool CEN_TIRPartialUnloading { get => base.CEN_TIRPartialUnloading; set => base.CEN_TIRPartialUnloading = value; }

		[ResourceStringData("ES.CusESNctsHeader.CEN_TIRCarnetPage", Caption = "TIR Carnet Page")]
		[MaxLength(Schema.TIRCarnetPageMaxLength)]
		public override ZShort CEN_TIRCarnetPage { get => base.CEN_TIRCarnetPage; set => base.CEN_TIRCarnetPage = value; }

		[List(nameof(Header) + "." + nameof(NctsHeader.Lookups) + "." + nameof(NctsHeaderLookups.TADPrintProcedureList))]
		public override ZString CEN_TADPrintProcedure
		{
			get => base.CEN_TADPrintProcedure;
			set
			{
				var oldValue = CEN_TADPrintProcedure;
				base.CEN_TADPrintProcedure = value;
				if (!IsCopying && oldValue != CEN_TADPrintProcedure)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		public override ZString CEN_ClearanceCriteria
		{
			get => base.CEN_ClearanceCriteria;
			set
			{
				var oldValue = CEN_ClearanceCriteria;
				base.CEN_ClearanceCriteria = value;
				if (!IsCopying && oldValue != CEN_ClearanceCriteria)
				{
					Header.MarkAsNeedingValidation();
				}
			}
		}

		[ResourceStringData("ES.CusESNctsHeader.CEN_AutomaticCompletion", Caption = "Automatic Completion")]
		public override ZBool CEN_AutomaticCompletion { get => base.CEN_AutomaticCompletion; set => base.CEN_AutomaticCompletion = value; }

		[ResourceStringData("ES.CusESNctsHeader.CEN_AutomaticTranshipment", Caption = "Automatic Transhipment")]
		public override ZBool CEN_AutomaticTranshipment { get => base.CEN_AutomaticTranshipment; set => base.CEN_AutomaticTranshipment = value; }
	}
}
