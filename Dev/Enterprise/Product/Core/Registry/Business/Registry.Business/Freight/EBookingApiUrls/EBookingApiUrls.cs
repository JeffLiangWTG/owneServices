using System.Xml;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class EBookingApiUrls : RegistryBusinessObjectTemplate
	{
		#region Schema

		protected abstract class Schema
		{
			public const string SelectedEBookingApiUrlCode = "SelectedEBookingApiUrlCode";
		}

		#endregion

		public EBookingApiUrls()
		{
		}

		public EBookingApiUrls(string defaultCode)
		{
			SelectedEBookingApiUrlCode = defaultCode;
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var result = new EBookingApiUrls();
			result.SelectedEBookingApiUrlCode = this.SelectedEBookingApiUrlCode;
			return result;
		}

		#endregion

		#region XML Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.SelectedEBookingApiUrlCode, SelectedEBookingApiUrlCode);
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			SelectedEBookingApiUrlCode = new ZString(reader.ReadElementString(Schema.SelectedEBookingApiUrlCode));
		}

		#endregion

		#region Property

		[List(nameof(EBookingApiUrlCodeList))]
		public ZString SelectedEBookingApiUrlCode
		{
			get { return selectedEBookingApiUrlCode; }
			set
			{
				if (!IsValidationSuspended)
				{
					ValidateSelectedEBookingApiUrlCode();
				}
				SetNonPersistentPropertyValue(SelectedEBookingApiUrlCodeInfo, ref selectedEBookingApiUrlCode, value);
			}
		}

		public ZPropertyInfo SelectedEBookingApiUrlCodeInfo
		{
			get { return GetZPropertyInfo(Schema.SelectedEBookingApiUrlCode); }
		}
		ZString selectedEBookingApiUrlCode;

		public ZString SelectedUrl
		{
			get
			{
				switch (selectedEBookingApiUrlCode)
				{
					case Constants.TestCode:
						return Constants.TestUrl;
					case Constants.ProdCode:
						return Constants.ProdUrl;
					default:
						return ZString.Empty;
				}
			}
		}

		#endregion

		#region Validation

		public void ValidateSelectedEBookingApiUrlCode()
		{
			ListValidation.ErrorIfInvalidCode(SelectedEBookingApiUrlCodeInfo, EBookingApiUrlCodeList);
		}

		#endregion

		#region Lookups

		public CodeDescriptionPairList EBookingApiUrlCodeList
		{
			get
			{
				if (eBookingApiUrlCodeList == null)
				{
					eBookingApiUrlCodeList = new CodeDescriptionPairList();

					eBookingApiUrlCodeList.AddPair(Constants.ProdCode, Constants.ProdDescription);
					eBookingApiUrlCodeList.AddPair(Constants.TestCode, Constants.TestDescription);
				}
				return eBookingApiUrlCodeList;
			}
		}

		CodeDescriptionPairList eBookingApiUrlCodeList;

		#endregion

		public static class Constants
		{
			public const string ProdCode = "PROD";
			public const string TestCode = "TEST";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Registry")]
			public const string ProdDescription = "Production URL";
			[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Registry")]
			public const string TestDescription = "Test URL";
			public const string ProdUrl = "https://abe.wisegrid.net/v1";
			public const string TestUrl = "https://abe-test.wisegrid.net/v1";
		}
	}
}
