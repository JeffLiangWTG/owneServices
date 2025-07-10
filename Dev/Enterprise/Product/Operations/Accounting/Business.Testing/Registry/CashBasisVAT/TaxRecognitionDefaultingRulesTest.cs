using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Accounting.Registry.Business.Testing
{
	[TestedType(typeof(TaxRecognitionDefaultingRules))]
	public class TaxRecognitionDefaultingRulesTest : RegistryBusinessObjectTemplateTestCase
	{
		public void TestFieldValidations()
		{
			var item = new TaxRecognitionDefaultingRules();

			Action<ZPropertyInfo, ZString> assert = (property, validValue) =>
			{
				property.Value = ZString.Empty;
				var expectedError = "Please enter a value.";
				AssertHasError(property, expectedError);

				using (item.GetValidationSuspender())
				{
					property.Value = (ZString)"WWW";
				}
				AssertHasError("The property should not be revalidated as validation was suspended.", property, expectedError);

				item.RunPreSaveValidation();
				AssertHasError(property, "Enter a valid selection.");

				property.Value = validValue;
				AssertNoErrors(property);
			};

			foreach (CodeDescriptionPair value in item.RecognitionTypes)
			{
				assert(item.APInputGoodsInfo, value.Code);
				assert(item.APInputServicesInfo, value.Code);
				assert(item.AROutputGoodsInfo, value.Code);
				assert(item.AROutputServicesInfo, value.Code);
			}
			foreach (CodeDescriptionPair value in item.OrganisationOverrideTypes)
			{
				assert(item.APOrganizationOverrideInfo, value.Code);
				assert(item.AROrganizationOverrideInfo, value.Code);
			}
		}

		public void TestIsCashBasisRecognition()
		{
			var item = new TaxRecognitionDefaultingRules();

			var goodsChargeCode = Factory.New<AccChargeCode>();
			goodsChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.GDS;
			var servicessChargeCode = Factory.New<AccChargeCode>();
			servicessChargeCode.AC_GoodsServiceType = GoodServiceTypes.Codes.SRV;

			var accrual = TaxRecognitionDefaultingRules.RecognitionTypesAccrualCode;
			var cash = TaxRecognitionDefaultingRules.RecognitionTypesCashCode;
			var values = new[] { accrual, cash };

			foreach (var apInputGoods in values)
			{
				foreach (var apInputServices in values)
				{
					foreach (var arOutputGoods in values)
					{
						foreach (var arOutputServices in values)
						{
							item.APInputGoods = apInputGoods;
							item.APInputServices = apInputServices;
							item.AROutputGoods = arOutputGoods;
							item.AROutputServices = arOutputServices;

							string message = string.Format("APInputGoods: {0}, APInputServices: {1}, AROutputGoods = {2}, AROutputServices = {3}", apInputGoods, apInputServices, arOutputGoods, arOutputServices);

							AssertEquals("For AP and charge code is null when " + message, item.APInputServices == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsPayable, null));
							AssertEquals("For AR and charge code is null when " + message, item.AROutputServices == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsReceivable, null));
							AssertEquals("For AP and goods charge code when " + message, item.APInputGoods == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsPayable, goodsChargeCode));
							AssertEquals("For AR and goods charge code when " + message, item.AROutputGoods == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsReceivable, goodsChargeCode));
							AssertEquals("For AP and servises charge code when " + message, item.APInputServices == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsPayable, servicessChargeCode));
							AssertEquals("For AR and servises charge code when " + message, item.AROutputServices == cash, item.IsCashBasisRecognition(LedgerTypes.AccountsReceivable, servicessChargeCode));
							var otherLedgers = from field in typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
											   let ledger = (string)field.GetValue(null)
											   where ledger != LedgerTypes.AccountsReceivable && ledger != LedgerTypes.AccountsPayable
											   select ledger;
							foreach (var ledger in otherLedgers)
							{
								AssertEquals(string.Format("For {0} and goods charge code when {1}", ledger, message), false, item.IsCashBasisRecognition(ledger, goodsChargeCode));
								AssertEquals(string.Format("For {0} and services charge code when {1}", ledger, message), false, item.IsCashBasisRecognition(ledger, servicessChargeCode));
							}
						}
					}
				}
			}
		}

		public void TestIsOrganisationOverridePermitted()
		{
			var item = new TaxRecognitionDefaultingRules();
			var no = TaxRecognitionDefaultingRules.OrganisationOverrideTypesNoCode;
			var yes = TaxRecognitionDefaultingRules.OrganisationOverrideTypesYesCode;
			var values = new[] { no, yes };

			foreach (var apOrganizationOverride in values)
			{
				foreach (var arOrganizationOverride in values)
				{
					item.APOrganizationOverride = apOrganizationOverride;
					item.AROrganizationOverride = arOrganizationOverride;

					string message = string.Format("APOrganizationOverride: {0}, AROrganizationOverride: {1}", apOrganizationOverride, arOrganizationOverride);

					AssertEquals("For AP when " + message, item.APOrganizationOverride == yes, item.IsOrganisationOverridePermitted(LedgerTypes.AccountsPayable));
					AssertEquals("For AR when " + message, item.AROrganizationOverride == yes, item.IsOrganisationOverridePermitted(LedgerTypes.AccountsReceivable));
					var otherLedgers = from field in typeof(LedgerTypes).GetFields(BindingFlags.Static | BindingFlags.Public)
									   let ledger = (string)field.GetValue(null)
									   where ledger != LedgerTypes.AccountsReceivable && ledger != LedgerTypes.AccountsPayable
									   select ledger;
					foreach (var ledger in otherLedgers)
					{
						AssertEquals(string.Format("For {0} when {1}", ledger, message), false, item.IsOrganisationOverridePermitted(ledger));
					}
				}
			}
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new TaxRecognitionDefaultingRules();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToClone()
		{
			return new TaxRecognitionDefaultingRules();
		}

		protected override RegistryBusinessObjectTemplate GetBusinessObjectToSerialise()
		{
			return GetBusinessObjectToClone();
		}

		protected override bool RequiresFactory
		{
			get { return true; }
		}

		protected override bool RequiresFallbackLevel
		{
			get { return true; }
		}

		#endregion
	}
}
