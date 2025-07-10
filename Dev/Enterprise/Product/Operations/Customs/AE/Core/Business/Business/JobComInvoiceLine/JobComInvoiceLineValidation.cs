using CargoWise.EntityFramework;

namespace Enterprise.Customs.AE.Business;

public class JobComInvoiceLineValidation : AutoAEJobComInvoiceLineValidation
{
	public JobComInvoiceLineValidation(JobComInvoiceLine invoiceLine)
		: base(invoiceLine)
	{
	}

	#region CheckJI_Tariff
	protected override void CheckJI_Tariff()
	{
		if (Declaration != null)
		{
			if (Declaration.IsHighValue && !Declaration.IsTranshipment && Parent.JI_Tariff.IsEmpty)
			{
				Parent.JI_TariffInfo.AddMessageError(MessageErrorTariffMayNotBeEmpty);
			}
		}
		if (!Parent.JI_Tariff.IsEmpty && Parent.JI_Tariff.Replace(".", "").Length != 8)
		{
			Parent.JI_TariffInfo.AddMessageError(MessageErrorTariffShouldBe8DigitsLong);
		}

		if (ClassificationIsRequiredForAutoCreationOfProduct && !Parent.JI_PartNo.IsEmpty && Parent.Part == null && Parent.JI_CC.IsEmpty && Parent.JI_Tariff.IsEmpty)
		{
			Parent.JI_TariffInfo.AddWarning(MandatoryCCOrTariffForAutoCreateProduct);
		}
	}

	public const string MessageErrorTariffMayNotBeEmpty = "Tariff may not be empty";
	public const string MessageErrorTariffShouldBe8DigitsLong = "Tariff should be 8 digits long (ignoring the dots)";
	#endregion

	#region CheckJI_CountryOfOrigin
	protected override void CheckJI_CountryOfOrigin()
	{
		base.CheckJI_CountryOfOrigin();
		if (Parent.JI_CountryOfOrigin.IsEmpty && Parent.InvoiceHeader.DefaultOrigin == null)
		{
			MandatoryValidation.MessageErrorIfNotEntered(Parent.JI_CountryOfOriginInfo);
		}
	}
	#endregion

	protected override void CheckJI_NewUsed()
	{
		base.CheckJI_NewUsed();
		ListValidation.MessageErrorIfInvalidCodeOrEmpty(Parent.JI_NewUsedInfo);
	}

	#region Typed Properties
	#region Parent
	protected new JobComInvoiceLine Parent
	{
		get { return (JobComInvoiceLine)base.Parent; }
	}
	#endregion

	#region Declaration
	public new JobDeclaration Declaration => (JobDeclaration)base.Declaration;
	#endregion
	#endregion
}
