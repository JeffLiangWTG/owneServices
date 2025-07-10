using CargoWise.EntityFramework;
using CargoWise.Types;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using CargoWiseOne.ResourceStrings;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class JobComInvoiceLineValidation : Customs.Business.BaseJobComInvoiceLineValidation
	{
		public JobComInvoiceLineValidation(JobComInvoiceLine parent)
			: base(parent)
		{
		}

		public new JobComInvoiceLine Parent
		{
			get { return (JobComInvoiceLine)base.Parent; }
		}

		protected JobComInvoiceLineLookups Lookups
		{
			get { return Parent.Lookups; }
		}

		protected override bool IsTariffMandatory
		{
			get { return false; }
		}

		public override void ValidateAll()
		{
			base.ValidateAll();
			ValidateDangerousGoodsDGSubs();
		}

		public void ValidateDangerousGoodsDGSubs()
		{
			ValidateCalculatedProperty(Parent.DangerousGoodsDGSubsInfo);
		}

		protected void CheckDangerousGoodsDGSubs()
		{
			ListValidation.ErrorIfInvalidPK(Parent.DangerousGoodsDGSubsInfo);
		}

		protected override void CheckJI_PartAttrib1()
		{
			base.CheckJI_PartAttrib1();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_PartAttrib2()
		{
			base.CheckJI_PartAttrib2();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_PartAttrib3()
		{
			base.CheckJI_PartAttrib3();
			ValidateJI_PartNo();
		}

		protected override void CheckJI_Description()
		{
			base.CheckJI_Description();
			if (Parent.JI_Description.HasCharactersNotSupportedByCAMessaging())
			{
				Parent.JI_DescriptionInfo.AddWarning(Res.GetString("2f629ded-6ff3-4666-8c88-5b006a052865", "The Description has special characters which cannot be sent in the EDI message. These will be stripped when the messages is sent to Customs."));
			}
		}

		protected override void CheckJI_WeightUQ()
		{
			base.CheckJI_WeightUQ();
			if (Parent.GACPGAHeader.CA_AllProgramInd == Customs.Business.YesNoList.Codes.Yes)
			{
				if (Parent.JI_CustomsQuantity == ZDecimal.Zero && Parent.JI_WeightUQ != Core.Constants.Weight.Kilograms)
				{
					Parent.JI_WeightUQInfo.AddMessageError(Res.GetString("c76db803-e931-4432-80df-f34bee294012", "Unit of Measure must be KGM if PGA - Global Affairs Canada is being reported."));
				}
			}
		}
		protected override bool ClassificationIsRequiredForAutoCreationOfProduct
		{
			get { return false; }
		}
	}
}
