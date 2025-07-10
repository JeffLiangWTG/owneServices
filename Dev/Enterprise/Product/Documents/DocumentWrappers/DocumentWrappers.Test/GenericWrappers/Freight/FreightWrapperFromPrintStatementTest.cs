using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.DocumentWrappers.GenericWrappers.Base;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentWrappers.GenericWrappers.Testing
{
	[TestedType(typeof(FreightWrapperFromPrintStatement))]
	sealed class FreightWrapperFromPrintStatementTest : FreightWrapperTest
	{
		public override void TestTrackingBusinessObjectPK()
		{
			var wrapper = new FreightWrapperFromPrintStatement(PrintStatement, Factory);
			AssertEquals("TrackingBusinessObjectPK", PrintStatement.PK, wrapper.TrackingBusinessObjectPK);
		}

		#region Implementation

		protected override GenericWrapper GetSetupWrapperForDefaultFormatting()
		{
			return new FreightWrapperFromPrintStatement(PrintStatement, Factory);
		}

		protected override BusinessObject GetNewBusinessObjectToWrap()
		{
			return new PrintStatement(Factory, GlbBranch.CurrentBranch);
		}

		protected override bool IsCarrierUsed
		{
			get
			{
				return false;
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			PrintStatement = new PrintStatement(Factory, GlbBranch.CurrentBranch);
		}

		PrintStatement PrintStatement;

		#endregion
	}
}
