using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Client.JAS.Business.Invoicing;
using Enterprise.Environment;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.JAS.Business.JXC.Export.Testing
{
	internal class INVDLineTest : MessageLineTestCase
	{
		public void TestConstructor()
		{
			INVDLine iNVDLine = (INVDLine)Line;
			AssertEquals("Should be assigned in the constructor", ARInvoiceLine, iNVDLine.InvoiceLine);
		}

		public void TestLineAsString_NullParams()
		{
			Line = new INVDLine(null);
			AssertEquals("INVD3100;;;;;;;", Line.LineAsString);
		}

		public void TestLineAsString()
		{
			AssertEquals("INVD3100;FRT;Description;200.1;PPN;HB101;CON101;SEAL101", Line.LineAsString);
		}

		public void TestLineAsString_NoContainer()
		{
			foreach (var container in Shipment.Containers.ToArray())
			{
				Shipment.UnpackFromContainer(container);
			}

			AssertEquals("INVD3100;FRT;Description;200.1;PPN;HB101;;", Line.LineAsString);
		}

		public void TestLineAsString_NoChargeCode()
		{
			ARInvoiceLine.GenericCharge = ZGuid.Empty;
			ARInvoiceLine.AL_OSAmount = 100;
			AssertEquals("INVD3100;;Description;100;PPN;HB101;CON101;SEAL101", Line.LineAsString);
		}

		public void TestLineAsString_ExcessivelyLongStringShouldBeTrimmed()
		{
			ARInvoiceLine.AL_Desc = "01234567890123456789012345678901234567890123456789";
			ARInvoiceLine.GenericChargeBizO.VC_Code = "012345";
			ARInvoice.Branch.Company.GC_RN_NKCountryCode = "JP";
			AssertEquals("INVD3100;0123;012345678901234567890123456789012345678901234;200.1;CON;HB101;CON101;SEAL101", Line.LineAsString);
		}

		public void TestLineAsString_LineHasDifferentShipmentJob()
		{
			var newShipment = Factory.New<JASForwardingShipment>();
			newShipment.JS_HouseBill = "HB102";
			var consol = newShipment.Consols.AddNew();
			var container = consol.Containers.AddNew();
			var packLine = (JASForwardingPackLine)newShipment.OuterPackLines.AddNew();
			packLine.Containers.AddNew();
			container.JC_ContainerNum = "CON102";
			container.JC_SealNum = "SEAL102";
			ARInvoiceLine.AL_JH = Factory.NewJobForTesting<JASJob>().PK;
			ARInvoiceLine.Job.JH_ParentID = newShipment.PK;
			ARInvoiceLine.AL_OSAmount = 100.59m;
			AssertEquals("INVD3100;FRT;Description;100.59;PPN;HB102;CON102;SEAL102", Line.LineAsString);
		}

		public void TestLineAsString_LineHasNoAssociatedJob()
		{
			ARInvoiceLine.AL_JH = ZGuid.Empty;
			ARInvoiceLine.AL_OSAmount = 900.38m;
			AssertEquals("INVD3100;FRT;Description;900.38;PPN;;;", Line.LineAsString);
		}

		#region Implementation
		protected override int ExpectedFieldCount
		{
			get
			{
				return JXCConstants.INVDFieldCount;
			}
		}

		protected override ZString ExpectedLineType
		{
			get
			{
				return JXCConstants.LineTypes.INVD;
			}
		}

		protected override MessageLine GetMessageLine()
		{
			return new INVDLine(ARInvoiceLine);
		}

		JASForwardingShipment Shipment
		{
			get
			{
				if (fShipment == null)
				{
					fShipment = Factory.New<JASForwardingShipment>();
					fShipment.JS_HouseBill = "HB101";
					var consol = (ForwardingConsol)fShipment.Consols.FirstOrDefault() ?? fShipment.Consols.AddNew();
					var container = consol.Containers.AddNew();
					var packLine = (JASForwardingPackLine)fShipment.OuterPackLines.AddNew();
					packLine.Containers.Add(container);
					container.JC_ContainerNum = "CON101";
					container.JC_SealNum = "SEAL101";
				}

				return fShipment;
			}
		}

		ARInvoiceLine ARInvoiceLine
		{
			get
			{
				if (fARInvoiceLine == null)
				{
					fARInvoiceLine = (ARInvoiceLine)ARInvoice.Lines.AddNew();
					fARInvoiceLine.AL_JH = Job.PK;
					fARInvoiceLine.GenericCharge = new ZGuid(Env.Registry.FreightChargeCode);
					fARInvoiceLine.AL_Desc = "Description";
					fARInvoiceLine.AL_OSAmount = 200.10m;
				}

				return fARInvoiceLine;
			}
		}

		JASARInvoice ARInvoice
		{
			get
			{
				if (fARInvoice == null)
				{
					fARInvoice = Factory.New<JASARInvoice>();
					fARInvoice.AH_JH = Job.PK;
					fARInvoice.AH_GB = Factory.New(typeof(GlbBranch)).PK;
					fARInvoice.Branch.GB_GC = Factory.New(typeof(GlbCompany)).PK;
					fARInvoice.Branch.Company.GC_RN_NKCountryCode = "ID";
				}

				return fARInvoice;
			}
		}

		JASJob Job
		{
			get
			{
				if (fJob == null)
				{
					fJob = Factory.NewJobForTesting<JASJob>();
					fJob.JH_ParentID = Shipment.PK;
					fJob.JH_ParentTableCode = JobShipmentSchema.Constants.Prefix;
				}

				return fJob;
			}
		}

		JASForwardingShipment fShipment;
		JASARInvoice fARInvoice;
		ARInvoiceLine fARInvoiceLine;
		JASJob fJob;
		#endregion
	}
}
