using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentWrappers.Testing
{
	abstract class DocBaseConsolAbstractTestClass : DocumentWrapperTestCase
	{
		#region Virtual Test Methods

		public virtual void TestPortOfDischarge()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertNull("Port of discharge", baseConsolWrapper.PortOfDischarge);

			RefUNLOCO uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, "USCHI"));
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, uNLOCO.RL_RN_NKCountryCode));
			transport.JW_RL_NKDiscPort = uNLOCO.RL_Code;
			AssertEquals("PortOfDischarge", typeof(DocUNLOCO), baseConsolWrapper.PortOfDischarge.GetType());
			AssertEquals("PortOfDischarge Name", uNLOCO.RL_PortName, baseConsolWrapper.PortOfDischarge.PortName);
		}

		public virtual void TestPortOfLoading()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertNull("Port of loading is empty", baseConsolWrapper.PortOfLoading);

			var uNLOCO = Factory.LoadTop1<RefUNLOCO>(new ZQuery(RefUNLOCOSchema.RL_Code, GlbBranch.CurrentBranch.GB_RL_NKHomePort));
			RefCountry country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, uNLOCO.RL_RN_NKCountryCode));
			transport.JW_RL_NKLoadPort = uNLOCO.RL_Code;
			AssertEquals("PortOfLoading", typeof(DocUNLOCO), baseConsolWrapper.PortOfLoading.GetType());
			AssertEquals("PortOfLoading Port Code", uNLOCO.RL_PortName, baseConsolWrapper.PortOfLoading.PortName);
		}

		public virtual void TestETA()
		{
			var consol = Factory.New<CommonConsol>();
			Transport transport = consol.Transports[0];
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ETA is empty", ZDateTime.Empty, baseConsolWrapper.ETA);

			transport.JW_ETA = ZDateTime.Today;
			AssertEquals("ETA is not empty", ZDateTime.Today, baseConsolWrapper.ETA);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public virtual void TestETAString()
		{
			var consol = Factory.New<CommonConsol>();
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ETA is empty", ZString.Empty, baseConsolWrapper.ETAString);

			Transport transport = consol.Transports[0];
			transport.JW_ETA = ZDateTime.Today;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ETA only shows date", ZDateTime.Today.ToString("dd-MMM-yy"), baseConsolWrapper.ETAString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ETA shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETAString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ETA shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETAString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("ETA shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETAString);
		}

		public virtual void TestETD()
		{
			var consol = Factory.New<CommonConsol>();
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ETD is empty", ZDateTime.Empty, baseConsolWrapper.ETD);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			AssertEquals("ETD is not empty", ZDateTime.Today, baseConsolWrapper.ETD);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public virtual void TestETDString()
		{
			var consol = Factory.New<CommonConsol>();
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ETD is empty", ZString.Empty, baseConsolWrapper.ETDString);

			Transport transport = consol.Transports[0];
			transport.JW_ETD = ZDateTime.Today;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ETD only shows date", ZDateTime.Today.ToString("dd-MMM-yy"), baseConsolWrapper.ETDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ETD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ETD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("ETD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ETDString);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public virtual void TestATDString()
		{
			var consol = Factory.New<CommonConsol>();
			DocBaseConsolTestClass baseConsolWrapper = new DocBaseConsolTestClass(consol, Factory);

			AssertEquals("ATD is empty", ZString.Empty, baseConsolWrapper.ATDString);

			Transport transport = consol.Transports[0];
			transport.JW_ATD = ZDateTime.Today;
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			AssertEquals("ATD only shows date", ZDateTime.Today.ToString("dd-MMM-yy"), baseConsolWrapper.ATDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Air;
			AssertEquals("ATD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ATDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Road;
			AssertEquals("ATD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ATDString);

			consol.JK_TransportMode = Core.Constants.TransportModes.Rail;
			AssertEquals("ATD shows date and time", ZDateTime.Today.ToLongTimeString(), baseConsolWrapper.ATDString);
		}

		#endregion
	}
}
