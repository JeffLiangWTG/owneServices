using System;
using System.Data;
using System.Linq;
using CargoWise.Data;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Common.Testing
{
	class IncotermPrepaidOrCollectIntegrationTest : TransactionedTestCase
	{
		public void TestSupportedIncoterms()
		{
			foreach (string incoterm in Constants.IncoTerms.Incoterms2000.Union(Constants.IncoTerms.Incoterms2010).Union(Constants.IncoTerms.Incoterms2020))
			{
				string expected = GetIncotermPrepaidOrCollectFromLocal(incoterm);
				string actual = GetIncotermPrepaidOrCollectFromSQL(incoterm);

				AssertNotEquals(string.Format("test method IncotermPrepaidOrCollectCode does not cover {0}", incoterm), "", expected);
				AssertNotEquals(string.Format("db function IncotermPrepaidOrCollect does not cover {0}", incoterm), "", actual);
				AssertEquals(expected, actual);
			}
		}

		#region Implementation

		bool IsCollect(string incoterm)
		{
			return incoterm == Constants.IncoTerms.ExWorks ||
				incoterm == Constants.IncoTerms.FreeCarrier ||
				incoterm == Constants.IncoTerms.FreeCarrierSeller ||
				incoterm == Constants.IncoTerms.FreeCarrierBuyer ||
				incoterm == Constants.IncoTerms.FreeAlongsideShip ||
				incoterm == Constants.IncoTerms.FreeOnBoard ||
				incoterm == Constants.DomesticPaymentTerms.Collect ||
				incoterm == Constants.DomesticPaymentTerms.CollectCOD ||
				incoterm == Constants.DomesticPaymentTerms.CollectThirdParty;
		}

		bool IsPrepaid(string incoterm)
		{
			return incoterm == Constants.IncoTerms.CostAndFreight ||
				incoterm == Constants.IncoTerms.CostInsuranceAndFreight ||
				incoterm == Constants.IncoTerms.CarriagePaidTo ||
				incoterm == Constants.IncoTerms.CarriageAndInsurancePaidTo ||
				incoterm == Constants.IncoTerms.DeliveredAtFrontier ||
				incoterm == Constants.IncoTerms.DeliveredExShip ||
				incoterm == Constants.IncoTerms.DeliveredExQuay ||
				incoterm == Constants.IncoTerms.DeliveredDutyUnpaid ||
				incoterm == Constants.IncoTerms.DeliveredDutyPaid ||
				incoterm == Constants.IncoTerms.DeliveredAtPlace ||
				incoterm == Constants.IncoTerms.DeliveredAtTerminal ||
				incoterm == Constants.IncoTerms.DeliveredAtPlaceUnloaded ||
				incoterm == Constants.DomesticPaymentTerms.Prepaid;
		}

		string GetIncotermPrepaidOrCollectFromLocal(string incoterm)
		{
			string paymentType = string.Empty;

			if (IsCollect(incoterm))
			{
				paymentType = Constants.PaymentType.Collect;
			}
			else if (IsPrepaid(incoterm))
			{
				paymentType = Constants.PaymentType.Prepaid;
			}

			return paymentType;
		}

		string GetIncotermPrepaidOrCollectFromSQL(string incoterm)
		{
			const string sql = "select value from dbo.IncotermPrepaidOrCollect(@incoterm)";
			using (DbCommand command = Db.Connection.Command(sql))
			{
				command.AddParameter("@incoterm", SqlDbType.VarChar, incoterm);
				return Convert.ToString(command.ExecuteScalar());
			}
		}
		#endregion
	}
}

