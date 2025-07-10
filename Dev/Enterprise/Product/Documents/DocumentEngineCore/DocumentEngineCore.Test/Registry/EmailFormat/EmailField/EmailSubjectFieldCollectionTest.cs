using CargoWise.EntityFramework;
using NUnit.Framework;

namespace Enterprise.DocumentEngineCore.Registry.Testing
{
	[TestedType(typeof(EmailSubjectFieldCollection))]
	public class EmailSubjectFieldCollectionTest : EmailFieldCollectionTest<EmailSubjectFieldCollection>
	{
		#region Implementation

		protected override EmailSubjectFieldCollection GetCollectionToTest()
		{
			return new EmailSubjectFieldCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new EmailSubjectField();
		}

		#endregion
	}
}
