using System.Linq;
using System.Reflection;
using NUnit.Framework;

namespace Enterprise.Core.Testing
{
	sealed class DocManagerCodesTest : TestCase
	{
		public void TestDocManagerCodesAllUnique()
		{
			var codeFields = typeof(Constants.DocManagerCodes).GetFields(BindingFlags.Static | BindingFlags.Public);

			var allCodes = (from field in codeFields
							group field by field.GetValue(null) into g
							select g).ToList();

			Assert("Not all DocManagerCodes are unique", allCodes.All((x) => x.Count() == 1));
		}
	}
}
