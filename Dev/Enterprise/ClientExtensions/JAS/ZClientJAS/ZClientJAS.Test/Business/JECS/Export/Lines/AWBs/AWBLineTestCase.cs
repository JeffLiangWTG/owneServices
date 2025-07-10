using System;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business.AWB;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal abstract class AWBLineTestCase : MessageLineTestCase
	{
		public abstract void TestLineAsString();

		public abstract void TestShipperAddressIsTrimmed();

		public abstract void TestConsigneeAddressIsTrimmed();

		public abstract void TestCarrierAddressIsTrimmed();

		public abstract void TestAgentPlaceIsTrimmed();

		public abstract void TestShipperAccountNoIsNeverEmpty();

		public abstract void TestConsigneeAccountNoIsNeverEmpty();

		public abstract void TestAccountingInfo();

		public abstract void TestAccountingInfoIsTrimmed();

		public abstract void TestHandlingInfo();

		protected virtual ZString GetExpectedLineAsStringForTestShipperAddressIsTrimmed(ZString shipperAddress1, ZString shipperAddress2)
		{
			string expectedShipperAddressString = ";" + shipperAddress1.Left(ExpectedShipperAddressMaxLength) + ";" + shipperAddress2.Left(ExpectedShipperAddressMaxLength) + ";";
			return ExpectedLineAsString.Replace(";Stairway;to heaven;", expectedShipperAddressString);
		}

		protected virtual ZString GetExpectedLineAsStringForTestConsigneeAddressIsTrimmed(ZString consigneeAddress1, ZString consigneeAddress2)
		{
			string expectedConsigneeAddressString = ";" + consigneeAddress1.Left(ExpectedConsigneeAddressMaxLength) + ";" + consigneeAddress2.Left(ExpectedConsigneeAddressMaxLength) + ";";
			return ExpectedLineAsString.Replace(";CStairway;Cto heaven;", expectedConsigneeAddressString);
		}

		#region Abstract
		protected abstract AWBLine GetAWBLine();
		protected abstract Type TypeOfExportAWBHeader { get; }

		protected abstract string ExpectedContentLineAsString { get; }

		protected abstract int ExpectedShipperAddressMaxLength { get; }

		protected abstract int ExpectedConsigneeAddressMaxLength { get; }

		protected abstract int ExpectedCarrierAddressMaxLength { get; }

		#endregion
		#region Implementation
		protected override MessageLine GetMessageLine()
		{
			return GetAWBLine();
		}

		protected ZString ExpectedLineAsString
		{
			get
			{
				return Line.LineIdentifier + JXCConstants.Delimiter + ExpectedContentLineAsString;
			}
		}

		#region Create AWBHeader for test
		protected abstract ExportAWBHeader AWBHeader { get; }
		#endregion
		#endregion
	}
}
