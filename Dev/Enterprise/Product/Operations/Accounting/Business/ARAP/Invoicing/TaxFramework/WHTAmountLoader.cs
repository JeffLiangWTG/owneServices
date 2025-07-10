using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.TaxFramework.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing.TaxFramework
{
	public interface IWHTAmountLoader
	{
		void RegisterForLoadingWHTAmounts(params ZGuid[] transactionPKs);

		ZDecimal GetRealizedWHT(ZGuid transactionPK);

		ZDecimal GetNotionalWHT(ZGuid transactionPK);

		bool IsWHTRealizationInProgress { get; set; }
	}

	public class WHTAmountLoader : IWHTAmountLoader
	{
		public WHTAmountLoader(BusinessObjectFactory factory)
		{
			TransactionPKs = new HashSet<ZGuid>();
			TaxTransactionBasedWHTAmountCalculator = ObjectFactory.Get<IWHTAmountCalculator>();
			Factory = factory;
		}

		public void RegisterForLoadingWHTAmounts(params ZGuid[] transactionPKs)
		{
			foreach (var pk in transactionPKs)
			{
				TransactionPKs.Add(pk);
			}
		}

		public ZDecimal GetRealizedWHT(ZGuid transactionPK) =>
			(IsWHTRealizationInProgress ? TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(Factory).CalculateRealizedWHT(transactionPK) : default) ?? GetWithheldTaxAmount(transactionPK).RealizedWHT;

		public ZDecimal GetNotionalWHT(ZGuid transactionPK) =>
			(IsWHTRealizationInProgress ? TaxFrameworkObjectFactory.GetAPJournalBasedWHTAmountCalculator(Factory).CalculateNotionalWHT(transactionPK) : default) ?? GetWithheldTaxAmount(transactionPK).NotionalWHT;

		public bool IsWHTRealizationInProgress { get; set; }

		(ZDecimal RealizedWHT, ZDecimal NotionalWHT) GetWithheldTaxAmount(ZGuid transactionPK)
		{
			var amountsWithPK = WithheldTaxes.GetOrAdd(transactionPK, () =>
			{
				RegisterForLoadingWHTAmounts(new ZGuid[] { transactionPK });
				Load();
				return WithheldTaxes[transactionPK];
			});

			return (amountsWithPK.RealizedAmount, amountsWithPK.NotionalAmount);
		}

		void Load()
		{
			var whtAmountsByTransactionPKs = TaxTransactionBasedWHTAmountCalculator.Calculate(TransactionPKs.ToArray());

			foreach (var whtAmountsByTransactionPK in whtAmountsByTransactionPKs)
			{
				if (WithheldTaxes.ContainsKey(whtAmountsByTransactionPK.TransactionPK))
				{
					WithheldTaxes[whtAmountsByTransactionPK.TransactionPK] = whtAmountsByTransactionPK;
				}
				else
				{
					WithheldTaxes.Add(whtAmountsByTransactionPK.TransactionPK, whtAmountsByTransactionPK);
				}
			}

			TransactionPKs.Clear();
		}

		Dictionary<ZGuid, IWithheldTaxAmounts> WithheldTaxes => withheldTaxes ?? (withheldTaxes = new Dictionary<ZGuid, IWithheldTaxAmounts>());
		Dictionary<ZGuid, IWithheldTaxAmounts> withheldTaxes;

		HashSet<ZGuid> TransactionPKs { get; }

		BusinessObjectFactory Factory { get; }

		IWHTAmountCalculator TaxTransactionBasedWHTAmountCalculator { get; }
	}
}