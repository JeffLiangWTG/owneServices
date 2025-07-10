using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BarcodeParsingEngine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	class BarcodeParsingConsumerTest : BarcodeParsingTestCase
	{
		#region TestBuyerCaption

		public void TestBuyerCaption()
		{
			AssertNull("Buyer Caption is null by default and when null is returned, the BarcodeRuleSet will use the Resource string XML.",
				((IBarcodeParsingConsumer)new DummyConsumer(Factory)).BuyerCaption);
		}

		#endregion

		#region TestConstructor

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () => new DummyConsumer(null));
		}

		#endregion

		#region TestFactory

		public void TestFactory()
		{
			AssertEquals(Factory, new DummyConsumer(Factory).Factory);
		}

		#endregion

		#region TestRelatedEntityCaption

		public void TestRelatedEntityCaption()
		{
			AssertNull("Related Entity Caption is null by default and when null is returned, the BarcodeRuleSet will use the Resource string XML.",
				((IBarcodeParsingConsumer)new DummyConsumer(Factory)).RelatedEntityCaption);
		}

		#endregion

		#region TestRelatedEntityRequirements

		public void TestRelatedEntityRequirements()
		{
			AssertEquals(RelatedEntityRequirements.None, ((IBarcodeParsingConsumer)new DummyConsumer(Factory)).RelatedEntityRequirements);
		}

		#endregion

		#region TestSupplierCaption

		public void TestSupplierCaption()
		{
			AssertNull("Supplier Caption is null by default and when null is returned, the BarcodeRuleSet will use the Resource string XML.",
				((IBarcodeParsingConsumer)new DummyConsumer(Factory)).SupplierCaption);
		}

		#endregion

		#region Implementation

		class DummyConsumer : BarcodeParsingConsumer<DummyTargetField>
		{
			public DummyConsumer(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			protected override ZString ModuleCode
			{
				get { throw new NotImplementedException(); }
			}

			protected override OrgHeaderCollection Buyers
			{
				get { throw new NotImplementedException(); }
			}

			protected override OrgHeaderCollection Suppliers
			{
				get { throw new NotImplementedException(); }
			}

			protected override IBusinessObjectCollection GetRelatedEntityList(OrgHeader buyer, OrgHeader supplier)
			{
				throw new NotImplementedException();
			}

			protected override ReadOnlyCodeDescriptionPairList TargetFields
			{
				get { throw new NotImplementedException(); }
			}

			protected override IEnumerable<ZString> GS1TargetFieldsToDefault
			{
				get { throw new NotImplementedException(); }
			}

			protected override IEnumerable<FormatType> GetValidFieldFormatsForTargetField(ZBool isGS1, ZString targetField)
			{
				throw new NotImplementedException();
			}

			protected override bool IsRelatedEntityAvailable
			{
				get { throw new NotImplementedException(); }
			}

			protected override bool IsBuyerAvailable
			{
				get { throw new NotImplementedException(); }
			}

			protected override bool IsSupplierAvailable
			{
				get { throw new NotImplementedException(); }
			}
		}

		#endregion
	}
}
