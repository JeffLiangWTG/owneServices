using System.IO;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture;

namespace Enterprise.Client.FSH.TsManifest
{
	class FortuneShippingFlatFileConverterTest : TestCaseWithFactory
	{
		public void TestPreprocessData()
		{
			string inputData = @"41:1:GE:4155:PK:PACKAGES:33710:0:348.05'
44:RYOBI AUST.
:P.O.NO.?:
:MODEL NO.?:
:QTY?:
:PORT?:
:C/NO.?:1-768
:OOG MERIDIAN
:FURNITURE
:E-4704'
47:3840 CARTON(S)
^RYOBI 30CC GRASS SCORPION
^LINE-
^PLT3043A
^315 CARTON(S)
^FURNITURE
^THIS SHIPMENT CONTAINS NO ANY
^SOLID WOOD PACKING MATERIALS.'
51:1:CCLU4030704:K940107:42G1:F:768:5145.6:3650:58.68::::::'".Replace("\n", System.Environment.NewLine);
			string expectedOutputData = @"41:1:GE:4155:PK:PACKAGES:33710:0:348.05'
44:RYOBI AUST.:P.O.NO.?::MODEL NO.?::QTY?::PORT?::C/NO.?:1-768:OOG MERIDIAN:FURNITURE:E-4704'
47:3840 CARTON(S) RYOBI 30CC GRASS SCORPION LINE- PLT3043A 315 CARTON(S) FURNITURE THIS SHIPMENT CONTAINS NO ANY SOLID WOOD PACKING MATERIALS.'
51:1:CCLU4030704:K940107:42G1:F:768:5145.6:3650:58.68::::::'".Replace("\n", System.Environment.NewLine);
			string output = Converter.PreprocessData(new StringReader(inputData));
			AssertMultilineASCIIEquals("Output from preprocessor", expectedOutputData, output);
		}

		#region Implementation
		FortuneShippingFlatFileConverter Converter
		{
			get
			{
				if (fConverter == null)
				{
					fConverter = new FortuneShippingFlatFileConverter(new NotificationBuffer(), Factory);
				}

				return fConverter;
			}
		}

		FortuneShippingFlatFileConverter fConverter;
		#endregion
	}
}
