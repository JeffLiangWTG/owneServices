using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.DocumentEngine.RuntimeOptions
{
	public abstract class ColumnSettingXMLVersionUpgrader
	{
		public string Upgrade(string xML)
		{
			string result = xML;
			if (!IsCurrentVersion(xML))
			{
				if (PreviousVersion != null)
				{
					result = PreviousVersion.Upgrade(xML);
				}
				else
				{
					result = UpgradeFromLastVersion(xML);
				}
			}
			return result;
		}

		ColumnSettingXMLVersionUpgrader PreviousVersion
		{
			get
			{
				if (fPreviousVersion == null)
				{
					fPreviousVersion = GetPreviousVersion();
				}
				return fPreviousVersion;
			}
		}
		ColumnSettingXMLVersionUpgrader fPreviousVersion;

		protected abstract ColumnSettingXMLVersionUpgrader GetPreviousVersion();
		protected abstract string UpgradeFromLastVersion(string xML);
		protected abstract bool IsCurrentVersion(string xML);
	}

	[XmlSerializerAssembly("Enterprise.DocumentEngine.XmlSerializers")]
	public abstract class ColumnSettingXMLVersionUpgraderGeneric<OldClassT, NewClassT, ParametersT> : ColumnSettingXMLVersionUpgrader where ParametersT : ColumnSettingXMLVersionUpgraderCurrentVersionParameters
	{
		protected ColumnSettingXMLVersionUpgraderGeneric(ParametersT parameters)
		{
		}

		override protected bool IsCurrentVersion(string xML)
		{
			bool result = false;

			using (StringReader stream = new StringReader(xML))
			using (XmlTextReader reader = new System.Xml.XmlTextReader(stream))
			{
				try
				{
					result = NewSerialiser.CanDeserialize(reader);
				}
				catch (XmlException)
				{
					result = false;
				}
			}

			return result;
		}

		protected abstract NewClassT GetNewClass();
		protected abstract NewClassT MapClasses(OldClassT oldClass, NewClassT newClass);

		sealed protected override string UpgradeFromLastVersion(string xML)
		{
			string xMLResult = "";
			OldClassT oldClass;
			NewClassT newClass;
			oldClass = (OldClassT)OldSerialiser.Deserialize(new StringReader(xML));

			newClass = MapClasses(oldClass, GetNewClass());

			StringWriter writer = new StringWriter();
			NewSerialiser.Serialize(writer, newClass);
			xMLResult = writer.ToString();

			return xMLResult;
		}

		ZXmlSerializer OldSerialiser
		{
			get
			{
				if (fSerialiserOld == null)
				{
					fSerialiserOld = ZXmlSerializer.New(typeof(OldClassT));
				}
				return fSerialiserOld;
			}
		}
		ZXmlSerializer fSerialiserOld;

		ZXmlSerializer NewSerialiser
		{
			get
			{
				if (fSerialiserNew == null)
				{
					fSerialiserNew = ZXmlSerializer.New(typeof(NewClassT));
				}
				return fSerialiserNew;
			}
		}
		ZXmlSerializer fSerialiserNew;
	}
}