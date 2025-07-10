using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DataConverters
{
	public class VesselDataWriter : DataWriter
	{
		public VesselDataWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region Public Fields to be filled in when importing

		public ZString VesselCode;
		public ZString VesselLloydsNo;
		public ZString VesselLine;
		public ZString VesselNationality;

		#endregion

		protected override internal ZString GetAnyReasonRecordShouldBeExcluded()
		{
			return (VesselCode.IsEmpty) ? new ZString("Doesn't have a name") : ZString.Empty ;
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			RefVessel newVessel = RefVessel.New(Factory);
			return newVessel;
		}

		protected override BusinessObject GetExistingBusinessObject()
		{
			RefVessel vessel = RefVessel.LookupVesselByLloyds(VesselLloydsNo, Factory);
			if (vessel == null && !VesselCode.IsEmpty)
			{
				vessel = RefVessel.LookupVesselByCode(VesselCode, Factory);
			}
			return vessel;
		}

		protected override void UpdateEnterpriseValues(BusinessObject businessObjectToUpdate)
		{
			RefVessel vessel = (RefVessel)businessObjectToUpdate;

			vessel.RV_Code = VesselCode;
			vessel.RV_LloydsNumber = VesselLloydsNo;
			//Vessel.RV_RN = GetCountryPK(VesselNationality);//TODO: when importing from another database if needed
		}

		public override ZString RecordDescription
		{
			get {	return new ZString("Vessel: " + VesselCode); }
		}

		protected override internal ZString CSVOutputLine
		{
			get { return new ZString(VesselCode + "," + ZString.Empty + "," + VesselLloydsNo);	}
		}
	}
}
