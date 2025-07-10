using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;

namespace Enterprise.DataConverters.CustomsFiles.AU
{
	public class ClassificationWriter : CustomsFiles.ClassificationWriter
	{
		#region Public Fields
		public ZString Treatment;
		public ZString InstrumentType;
		public ZString InstrumentCode;
		public ZString Preference;
		public ZString Origin;
		#endregion

		public ClassificationWriter(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region CSVOutputLine
		protected override internal ZString CSVOutputLine
		{
			get
			{
				return LookupCode + "," +
						ClassificationType + "," +
						Description + "," +
						TariffCode + "," +
						Treatment + "," +
						InstrumentType + "," +
						InstrumentCode; }
		}
		#endregion

		#region UpdateCountrySpecificData
		protected override void UpdateCountrySpecificData(Customs.Business.BaseCusClassification bizO)
		{
			Classification classification = (Classification)bizO;
			classification.CC_AddInfo = GetAddInfoString();
		}
		#endregion

		#region GetAddInfoString
		internal ZString GetAddInfoString()
		{
			ZString result = ZString.Empty;
			if (!InstrumentCode.IsEmpty)
			{
				result += "InstrumentCode_Hidden=" + InstrumentCode;
			}
			if (!InstrumentType.IsEmpty)
			{
				result += "*" + "InstrumentType_Hidden=" + InstrumentType;
			}
			if (!Treatment.IsEmpty)
			{
				result += "*" + "TreatmentCode_Hidden=" + Treatment;
			}
			if (!Preference.IsEmpty)
			{
				result += "*" + "PRF=" + Preference;
			}
			if (!Origin.IsEmpty)
			{
				result += "*" + "ORG=" + Origin;
			}
			return result;
		}
		#endregion

		#region GetClassificationType
		protected override Type GetClassificationType()
		{
			return typeof(Classification);
		}
		#endregion
	}
}
