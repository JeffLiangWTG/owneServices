using System;
using CargoWise.EntityFramework;

namespace Enterprise.Customs.BR.Business
{
	public class GoodsCatalogDownloadObjectValidation : ZValidation
	{
		public GoodsCatalogDownloadObjectValidation(GoodsCatalogDownloadObject goodsCatalogDownload) : base(goodsCatalogDownload)
		{
			parent = goodsCatalogDownload;
		}
		readonly GoodsCatalogDownloadObject parent;

		public override Type AutoValidationType => typeof(GoodsCatalogDownloadObjectValidation);

		public override void ValidateAll()
		{
			ValidateOwnerCode();
			ValidateBrokerCode();
			ValidateDownloadCatalog();
			ValidateDownloadForeignOperator();
		}

		public void ValidateOwnerCode()
		{
			ValidateCalculatedProperty(parent.OwnerCodeInfo);
		}

		public void ValidateBrokerCode()
		{
			ValidateCalculatedProperty(parent.BrokerCodeInfo);
		}

		public void ValidateDownloadCatalog()
		{
			ValidateCalculatedProperty(parent.DownloadCatalogInfo);
		}

		public void ValidateDownloadForeignOperator()
		{
			ValidateCalculatedProperty(parent.DownloadForeignOperatorInfo);
		}

		protected void CheckOwnerCode()
		{
			ListValidation.ErrorIfInvalidCodeOrEmpty(parent.OwnerCodeInfo);

			if (parent.Owner != null)
			{
				var cnpj = parent.Owner.GetRootCNPJ();

				if (!parent.Owner.OH_IsConsignee && !parent.Owner.OH_IsConsignor)
				{
					parent.OwnerCodeInfo.AddError(Res.GetString("71414D21-862E-42B4-807D-DE6CB0BC6B0B", "Organization is not set as Consignee or Consignor"));
				}

				if (cnpj.IsEmpty)
				{
					parent.OwnerCodeInfo.AddError(Res.GetString("75F3E749-AE3F-4CE6-A8FC-4B499FDA7578", "Organization is not set as Root CNPJ"));
				}
			}
		}

		protected void CheckBrokerCode()
		{
			MandatoryValidation.CheckEntered(parent.BrokerCodeInfo);
			ListValidation.ErrorIfInvalidCode(parent.BrokerCodeInfo);
			if (parent.Broker != null)
			{
				GlbExternalPasswordValidation_CCT.CheckValidCertificate(parent.BrokerCertificate, parent.BrokerCodeInfo);
			}
		}

		protected void CheckDownloadCatalog()
		{
			CheckDownloadOptions(parent.DownloadCatalogInfo);
		}

		protected void CheckDownloadForeignOperator()
		{
			CheckDownloadOptions(parent.DownloadForeignOperatorInfo);
		}

		void CheckDownloadOptions(ZPropertyInfo targetInfo)
		{
			if (!parent.DownloadCatalog && !parent.DownloadForeignOperator)
			{
				targetInfo.AddError(Res.GetString("826C7338-EB1A-462A-9483-E4975A0F4A37", "Please choose one of the options to Download Catalog"));
			}
		}
	}
}
