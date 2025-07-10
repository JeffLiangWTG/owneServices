using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Xml.XsdVersion1
{
	[XmlSerializerAssemblyAttribute("Enterprise.DataTransfer.XmlSerializers")]
	[XmlTypeAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/")]
	[XmlRootAttribute(Namespace = "http://www.edi.com.au/EnterpriseService/", IsNullable = false)]
	[ValueObjectSubclassAttribute("Enterprise.DataTransfer.Xml.XsdVersion1.AutoSailingWithVesselVoyage")]
	public class SailingWithVesselVoyage : AutoSailingWithVesselVoyage
	{
		[XmlIgnore]
		public override bool IsSpecified
		{
			get { return base.IsSpecified && (!VesselName.IsEmpty || !VoyageNo.IsEmpty || !LloydsNo.IsEmpty || !CargoCarrierCode.IsEmpty || FCLDates.IsSpecified || LCLDates.IsSpecified || (!ETA.IsEmpty && ETA.IsValid) || (!ETD.IsEmpty && ETD.IsValid) || (!LoadPortETA.IsEmpty && LoadPortETA.IsValid) || (!LoadPortATA.IsEmpty && LoadPortATA.IsValid)); }
		}

		public override CargoWise.Types.ZString GetVesselName(Enterprise.DataTransfer.Integration.IValueObjectImportContext context)
		{
			ZString result = VesselName;
			if (result.IsEmpty && !LloydsNo.IsEmpty)
			{
				RefVessel vessel = (RefVessel)context.Factory.LoadTop1(typeof(RefVessel), new ZQuery(RefVesselSchema.RV_LloydsNumber, LloydsNo));
				result = (vessel != null) ? vessel.RV_Code : ZString.Empty;
			}
			return result;
		}
	}
}
