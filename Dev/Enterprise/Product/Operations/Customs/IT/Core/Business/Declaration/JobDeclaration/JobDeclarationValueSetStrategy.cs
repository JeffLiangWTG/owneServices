using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Declaration;

public class JobDeclarationValueSetStrategy : EU.Business.Declaration.JobDeclarationValueSetStrategy
{
	public JobDeclarationValueSetStrategy(JobDeclaration declaration) : base(declaration)
	{
	}

	protected new JobDeclaration Declaration => (JobDeclaration)base.Declaration;

	protected override void ValueSetCore(ZPropertyInfo valueThatHasChanged, IZType oldValue)
	{
		base.ValueSetCore(valueThatHasChanged, oldValue);

		switch (valueThatHasChanged.Name)
		{
			case JobDeclaration.Schema.JE_OH_Supplier:
				OnSupplierChanged();
				break;
			case JobDeclaration.Schema.JE_OH_Importer:
				OnImporterChanged();
				break;
			case JobDeclaration.Schema.JE_LocationQualifier:
				OnLocationQualifierChanged();
				break;
			case JobDeclaration.Schema.JE_MessageType:
				OnMessageTypeChanged();
				break;
			case JobDeclaration.Schema.JE_OA_DeclarantAddress:
			case JobDeclaration.Schema.JE_DeclarantType:
				OnDeclarantChanged();
				break;
		}
	}

	protected override ZString GetNewCtStatusId() => Declaration.ZG_CTStatusID;

	#region JE_DeclarantType Defaulting Disabled

	protected override ZString GetDeclarantType(OrgHeader orgHeader)
	{
		return ZString.Empty;
	}

	#endregion

	void OnLocationQualifierChanged()
	{
		var locationQualifier = Declaration.JE_LocationQualifier;

		if (locationQualifier != ImportGoodsLocationQualifierWithAuthorisationList.Codes.AuthorizedPlace &&
			locationQualifier != ImportGoodsLocationQualifierWithAuthorisationList.Codes.ApprovedPlace)
		{
			var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(Declaration);
			defaulter.EmptyGoodsLocationSubFields();

			if (locationQualifier == GoodsLocationQualifierList.Codes.GoodsAreInCustomsAreaForInspection)
			{
				defaulter.AssignOfficeOfPresentationToLocationOfGoodsIfAvailable();
			}
		}
	}

	void OnMessageTypeChanged()
	{
		OnSupplierChanged();
		OnImporterChanged();
		new JobDeclarationGoodsLocationFieldsDefaulter(Declaration).EmptyGoodsLocationFields();
		AddJobComInvoiceLineAdditionalInfosMaxCountValidation();
	}

	void AddJobComInvoiceLineAdditionalInfosMaxCountValidation()
	{
		Invoices.ForEach(header => header.InvoiceLines.Cast<JobComInvoiceLine>().ForEach(line => line.EnableOrDisableAdditionalInfosMaxCountValidation()));
	}

	void OnSupplierChanged()
	{
		Invoices.ForEach(x =>
		{
			x.AeoCertificateManager.AddAeoCertificateFromSupplierIfNeeded();
			x.UpdateDefaultSupportingDocumentCountryToSupplierCountry();
		});
	}

	void OnImporterChanged() => Invoices.ForEach(x => x.AeoCertificateManager.AddAeoCertificateFromImporterIfNeeded());

	void OnDeclarantChanged() => Invoices.ForEach(x => x.AeoCertificateManager.AddAeoCertificateFromDeclarantIfNeeded());

	IEnumerable<JobComInvoiceHeader> Invoices => Declaration.Invoices.Cast<JobComInvoiceHeader>();
}
