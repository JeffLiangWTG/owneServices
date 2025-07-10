using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.MasterFiles.Business;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Uruguay
{
	public interface IEmisorBuilder
	{
		Emisor BuildEmisorInfo(TransactionInfo transaction, BusinessObjectFactory factory);
	}

	class EmisorBuilder : IEmisorBuilder
	{
		public EmisorBuilder()
		{
			transactionInfoHelper_constructorInitializedOnly = new TransactionInfoHelper();
		}

		Emisor IEmisorBuilder.BuildEmisorInfo(TransactionInfo transaction, BusinessObjectFactory factory)
		{
			var oEmisor = new Emisor();

			if (transaction?.BranchAddress != null)
			{
				oEmisor.RUCEmisor = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.RUT);

				if (transaction.BranchAddress.CompanyName.HasValue)
				{
					oEmisor.RznSoc = transaction.BranchAddress.CompanyName;
				}

				if (transaction.BranchAddress.Address1.HasValue)
				{
					oEmisor.DomFiscal = transaction.BranchAddress.Address1.Value.Substring(0, 70);
				}

				if (transaction.BranchAddress.City.HasValue)
				{
					oEmisor.Ciudad = transaction.BranchAddress.City.Value.Substring(0, 30);
				}

				if (transaction.BranchAddress.Country != null && transaction.BranchAddress.Country.Code.HasValue && ((ZString?)transaction.BranchAddress.State).HasValue)
				{
					var stateDescription = TransactionInfoHelper.GetStateDescriptionByCountry(transaction.BranchAddress.Country.Code, transaction.BranchAddress.State, factory);
					if (!stateDescription.IsNullOrEmpty())
					{
						oEmisor.Departamento = stateDescription.Length > 30 ? stateDescription.Substring(0, 30) : stateDescription;
					}
				}

				oEmisor.InfoAdicionalEmisor = GetInfoAdicional(transaction.BranchAddress);

				oEmisor.CdgDGISucur = TransactionInfoHelper.GetRegistrationCode(transaction.BranchAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.BRC);
			}

			return oEmisor;
		}

		string GetInfoAdicional(OrganizationAddress organizationAddress)
		{
			var infoAdicional = TransactionInfoHelper.GetRegistrationCode(organizationAddress, CountryCodes.Uruguay, UruguayOrgCusCodeInfo.OrgCusCodes.FZU);

			if (!infoAdicional.IsNullOrEmpty())
			{
				return "{ " + infoAdicional + (NoResString)"\n Contribuyente Amparado a la Ley N° 15.921 }";
			}

			return null;
		}

		ITransactionInfoHelper TransactionInfoHelper => transactionInfoHelper_constructorInitializedOnly;
		ITransactionInfoHelper transactionInfoHelper_constructorInitializedOnly;

#if DEBUG
		public void SubstituteTransactionInfoHelper_ForTestOnly(ITransactionInfoHelper replacement) => transactionInfoHelper_constructorInitializedOnly = replacement;
		public ITransactionInfoHelper TransactionInfoHelper_ExposedForTestOnly => TransactionInfoHelper;
#endif
	}
}
