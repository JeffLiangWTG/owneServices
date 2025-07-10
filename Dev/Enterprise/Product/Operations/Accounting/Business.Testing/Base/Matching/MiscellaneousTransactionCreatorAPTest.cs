using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(MiscellaneousTransactionCreatorAP))]
	public class MiscellaneousTransactionCreatorAPTest : MiscellaneousTransactionCreatorTest
	{
		protected override MiscellaneousTransactionCreator GetTestMiscTransCreator()
		{
			return new MiscellaneousTransactionCreatorAP(Factory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MiscellaneousTransactionCreatorAP(Factory);
		}

		#endregion
	}
}
