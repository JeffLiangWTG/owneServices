using CargoWise.Application;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.EInvoicingDependency;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico
{
	public interface ICFDiRelacionadosBuilder
	{
		ComprobanteCfdiRelacionados[] BuildRelacionadosInfo(TransactionInfo transaction);
	}

	class CFDiRelacionadosBuilder : ICFDiRelacionadosBuilder
	{
		readonly ITransactionInfoHelper Helper;

		public CFDiRelacionadosBuilder()
		{
			Helper = ObjectFactory.Get<IEInvoicingDependencyFactory>().GetTransactionInfoHelper();
		}

		ComprobanteCfdiRelacionados[] ICFDiRelacionadosBuilder.BuildRelacionadosInfo(TransactionInfo transaction)
		{
			var originalReference = Helper.GetGovernmentNumber(transaction.OriginalReference?.AuthorizationDetailCollection);

			if (!string.IsNullOrEmpty(originalReference))
			{
				var oRelacionados = new ComprobanteCfdiRelacionados[]
				{
					new ComprobanteCfdiRelacionados()
					{
						CfdiRelacionado = new ComprobanteCfdiRelacionadosCfdiRelacionado[]
						{
							new ComprobanteCfdiRelacionadosCfdiRelacionado() { UUID = originalReference }
						},
					}
				};

				var tipoRelacion = GetTipoRelacion(transaction.ComplianceSubType);
				if (tipoRelacion != null)
				{
					oRelacionados[0].TipoRelacion = tipoRelacion.Value;
				}

				return oRelacionados;
			}

			return null;
		}

		c_TipoRelacion? GetTipoRelacion(string complianceSubType)
		{
			switch (complianceSubType)
			{
				case MexicoComplianceInfo.ComplianceSubTypeCodes.TCR:
					return c_TipoRelacion.Item01;
				case MexicoComplianceInfo.ComplianceSubTypeCodes.TDR:
					return c_TipoRelacion.Item02;
				default:
					return null;
			}
		}
	}
}
