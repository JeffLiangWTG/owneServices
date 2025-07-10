using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.CC917C;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.ctypes;
using CargoWise.Customs.IE.MessageDefinitions.AESVersion1_0.tcl;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.IE.Messaging.AES.Testing
{
	class CC917CProviderTest : TestCaseWithFactory
	{
		public void TestLocalReferenceNumber()
		{
			AssertEquals("LRN917", Provider.LocalReferenceNumber);
		}

		public void TestMovementReferenceNumber()
		{
			AssertEquals("MRN917", Provider.MovementReferenceNumber);
		}

		public void TestErrors()
		{
			var errors = Provider.Errors.ToArray();

			var length = errors.Length;
			AssertEquals(3, length);

			var expected = new[]
			{
				("1", "1", "p1", "12", "Error Text 1"),
				("2", "2", "p2", "13", "Error Text 2"),
				("3", "3", "", "", "Error Text 3"),
			};

			for (int i = 0; i < expected.Length; i++)
			{
				CombineAssertions(() =>
				{
					AssertEquals(expected[i].Item1, errors[i].ErrorLineNumber);
					AssertEquals(expected[i].Item2, errors[i].ErrorColumnNumber);
					AssertEquals(expected[i].Item3, errors[i].ErrorPointer);
					AssertEquals(expected[i].Item4, errors[i].ErrorCode);
					AssertEquals(expected[i].Item5, errors[i].ErrorText);
				});
			}
		}

		CC917CProvider Provider
		{
			get
			{
				if (fProvider == null)
				{
					fProvider = new CC917CProvider(
						new Cc917C
						{
							Header = new HeaderType02
							{
								Lrn = "LRN917",
								Mrn = "MRN917",
							},
							XmlError = new Collection<XmlErrorType>
							{
								new XmlErrorType { ErrorLineNumber = "1", ErrorColumnNumber = "1", ErrorPointer = "p1", ErrorCode = XmlErrorCodes.Item12, ErrorText = "Error Text 1" },
								new XmlErrorType { ErrorLineNumber = "2", ErrorColumnNumber = "2", ErrorPointer = "p2", ErrorCode = XmlErrorCodes.Item13, ErrorText = "Error Text 2" },
								new XmlErrorType { ErrorLineNumber = "3", ErrorColumnNumber = "3", ErrorText = "Error Text 3" },
							}
						});
				}
				return fProvider;
			}
		}
		CC917CProvider fProvider;
	}
}
