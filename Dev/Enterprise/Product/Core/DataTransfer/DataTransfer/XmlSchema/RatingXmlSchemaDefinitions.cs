using System;
using System.Xml.Schema;

namespace Enterprise.DataTransfer.Xml
{
	public class RatingXmlSchemaDefinitions : XmlSchemaDefinitionsBase
	{
		#region Instance

		protected RatingXmlSchemaDefinitions()
		{
		}

		public static RatingXmlSchemaDefinitions Instance
		{
			get
			{
				RatingXmlSchemaDefinitions result = (RatingXmlSchemaDefinitions)WeakInstance.Target;
				if (result == null)
				{
					result = new RatingXmlSchemaDefinitions();
					WeakInstance.Target = result;
				}
				return result;
			}
		}

		static WeakReference WeakInstance
		{
			get { return weakInstance ?? (weakInstance = new WeakReference(null)); }
		}
		[ThreadStatic] static WeakReference weakInstance;

		#endregion

		[ExpectXmlSchemaContainsRootElement("Rates")]
		public XmlSchema RatesSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "Rate.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("Rate")]
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "May be an identifier or GUID.")]
		public XmlSchema SingleRateSchema
		{
			get { return GetCompiledSchemaNestedElement(RatesSchema, "Rates"); }
		}

		[ExpectXmlSchemaContainsRootElement("RateEntry")]
		public XmlSchema SingleRateEntrySchema
		{
			get { return GetCompiledSchemaWithElementOfType(RatesSchema, "RateEntry", "RateEntry"); }
		}

		[ExpectXmlSchemaContainsRootElement("RateLine")]
		public XmlSchema SingleRateLineSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RatesSchema, "RateLine", "RateLine"); }
		}

		#region RateCalculator
		[ExpectXmlSchemaContainsRootElement("")]
		public XmlSchema RateCalculatorSchema
		{
			get { return GetCompiledSchema(DefaultBaseResourceName, "RateCalculator.xsd"); }
		}

		[ExpectXmlSchemaContainsRootElement("CMBCalculator")]
		public XmlSchema CMBCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CMBCalculator", "CMBCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("CBICalculator")]
		public XmlSchema CBICalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CBICalculator", "CBICalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("AGYCalculator")]
		public XmlSchema AGYCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "AGYCalculator", "AGYCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("UNTCalculator")]
		public XmlSchema UNTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "UNTCalculator", "UNTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("FLTCalculator")]
		public XmlSchema FLTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "FLTCalculator", "FLTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("HRCCalculator")]
		public XmlSchema HRCCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "HRCCalculator", "HRCCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("MINCalculator")]
		public XmlSchema MINCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "MINCalculator", "MINCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("NTECalculator")]
		public XmlSchema NTECalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "NTECalculator", "NTECalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("CTZCalculator")]
		public XmlSchema CTZCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CTZCalculator", "CTZCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("CTBCalculator")]
		public XmlSchema CTBCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CTBCalculator", "CTBCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("CTGCalculator")]
		public XmlSchema CTGCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CTGCalculator", "CTGCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("EQHCalculator")]
		public XmlSchema EQHCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "EQHCalculator", "EQHCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("EXLCalculator")]
		public XmlSchema EXLCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "EXLCalculator", "EXLCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("WLTCalculator")]
		public XmlSchema WLTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "WLTCalculator", "WLTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("WPKCalculator")]
		public XmlSchema WPKCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "WPKCalculator", "WPKCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("TMECalculator")]
		public XmlSchema TMECalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "TMECalculator", "TMECalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("PERCalculator")]
		public XmlSchema PERCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "PERCalculator", "PERCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("PEBCalculator")]
		public XmlSchema PEBCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "PEBCalculator", "PEBCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("PSRCalculator")]
		public XmlSchema PSRCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "PSRCalculator", "PSRCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("MPUCalculator")]
		public XmlSchema MPUCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "MPUCalculator", "MPUCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("IXCCalculator")]
		public XmlSchema IXCCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "IXCCalculator", "IXCCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("SMBCalculator")]
		public XmlSchema SMBCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "SMBCalculator", "SMBCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("IATCalculator")]
		public XmlSchema IATCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "IATCalculator", "IATCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("FRTCalculator")]
		public XmlSchema FRTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "FRTCalculator", "FRTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("HRTCalculator")]
		public XmlSchema HRTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "HRTCalculator", "HRTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("FPUCalculator")]
		public XmlSchema FPUCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "FPUCalculator", "FPUCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("FPACalculator")]
		public XmlSchema FPACalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "FPACalculator", "FPACalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("DINCalculator")]
		public XmlSchema DINCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "DINCalculator", "DINCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("CSTCalculator")]
		public XmlSchema CSTCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "CSTCalculator", "CSTCalculator"); }
		}

		[ExpectXmlSchemaContainsRootElement("HCCCalculator")]
		public XmlSchema HCCCalculatorSchema
		{
			get { return GetCompiledSchemaWithElementOfType(RateCalculatorSchema, "HCCCalculator", "HCCCalculator"); }
		}

		#endregion

		const string DefaultBaseResourceName = "Enterprise.DataTransfer.DataFileDefinitions.Xml.Version1";
	}
}
