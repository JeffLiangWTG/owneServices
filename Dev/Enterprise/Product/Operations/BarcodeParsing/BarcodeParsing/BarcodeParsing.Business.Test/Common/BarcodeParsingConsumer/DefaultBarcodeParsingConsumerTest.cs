using System;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.BarcodeParsing.Business.Testing
{
	[TestedType(typeof(DefaultBarcodeParsingConsumer))]
	class DefaultBarcodeParsingConsumerTest : BarcodeParsingConsumerTestCase
	{
		#region Captions

		protected override string ExpectedBuyerCaption => null;
		protected override string ExpectedRelatedEntityCaption => null;
		protected override string ExpectedSupplierCaption => null;

		#endregion

		#region Collections

		protected override Type ExpectedTypeOfBuyers => typeof(ConsigneeCollection);
		protected override Type ExpectedTypeOfSuppliers => typeof(ConsignorCollection);

		#endregion

		#region Flags

		protected override bool ExpectedIsBuyerAvailable => false;
		protected override bool ExpectedIsRelatedEntityAvailable => false;
		protected override bool ExpectedIsSupplierAvailable => false;

		#endregion

		#region TestGS1TargetFieldsToDefault

		protected override ZString[] ExpectedGS1TargetFieldsToDefault => Array.Empty<ZString>();

		#endregion

		#region TestRelatedEntityRequirements

		protected override RelatedEntityRequirements ExpectedRelatedEntityRequirements => RelatedEntityRequirements.None;

		#endregion

		#region TestTargetFields

		protected override CodeDescriptionPairList ExpectedTargetFields => new CodeDescriptionPairList();

		#endregion
		#region Implementation

		protected override ZString ModuleCode
		{
			get { return ""; }
		}

		#endregion
	}
}
