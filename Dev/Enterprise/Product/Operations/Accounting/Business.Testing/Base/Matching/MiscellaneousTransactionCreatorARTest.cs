using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	[TestedType(typeof(MiscellaneousTransactionCreatorAR))]
	public class MiscellaneousTransactionCreatorARTest : MiscellaneousTransactionCreatorTest
	{
		protected override MiscellaneousTransactionCreator GetTestMiscTransCreator()
		{
			return new MiscellaneousTransactionCreatorAR(Factory);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			return new MiscellaneousTransactionCreatorAR(Factory);
		}

		#endregion
	}
}
