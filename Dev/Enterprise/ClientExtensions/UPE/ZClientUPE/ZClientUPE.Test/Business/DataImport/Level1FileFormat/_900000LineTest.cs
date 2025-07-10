using NUnit.Framework;

namespace Enterprise.Client.UPE.Business.DataImport.Level1FileFormat.Testing
{
	public class _900000LineTest : TestCase
	{
		public void TestARNRex()
		{
			AssertEquals("Input ARN Number doesn�t fit the ARN number format", false, _900000Line.ARNRex.IsMatch(" # ARN  : 65744051475swer"));
			AssertEquals("Input ARN Number doesn�t fit the ARN number format", false, _900000Line.ARNRex.IsMatch(" 123# ARN  :                                  65744051475swer#"));
			AssertEquals("Input ARN Number fit the ARN number format", true, _900000Line.ARNRex.IsMatch(" # ARN  : 65744051475swer#"));
			AssertEquals("Input ARN Number fit the ARN number format", true, _900000Line.ARNRex.IsMatch("#ARN:65744051475#"));
		}

		public void TestArnNumber()
		{
			AssertEquals("65744051475", _900000Line.ARNNumber);
		}

		protected override void SetUp()
		{
			base.SetUp();
			_900000Line = new _900000Line("US2795AU9639040422              D4A14T9J3YYD900000                                     18APR2004EDI                   #ARN:                              65744051475                    #                                                                                                                                                                                                 ");
			// #ARN:65744051475#   
		}

		_900000Line _900000Line;
	}
}
