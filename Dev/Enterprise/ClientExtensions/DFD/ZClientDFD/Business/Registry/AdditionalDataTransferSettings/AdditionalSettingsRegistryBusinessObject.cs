using System.Xml;
using System.Xml.Serialization;

using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.DFD.Registry
{
	[XmlSerializerAssembly("ZClientDFD.XmlSerializers")]
	public class AdditionalSettingsRegistryBusinessObject : DataTransferRegistryBusinessObject
	{
		public AdditionalSettingsRegistryBusinessObject()
			: base()
		{
		}

		public AdditionalSettingsRegistryBusinessObject(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Schema

		public new class Schema : DataTransferRegistryBusinessObject.Schema
		{
			public const string ExportFileName = "ExportFileName";
		}

		#endregion

		#region ExportFileName

		[CargoWise.ComponentModel.MaxLength(30)]
		public ZString ExportFileName
		{
			get { return exportFileName; }
			set
			{
				CheckMaximumLength(ExportFileNameInfo, value);
				exportFileName = value;
				SetPropertyValue(ExportFileNameInfo, value);
				if (!IsValidationSuspended)
				{
					ValidateExportFileName();
				}
				ExportFileNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExportFileNameInfo
		{
			get { return GetZPropertyInfo(Schema.ExportFileName); }
		}

		void ValidateExportFileName()
		{
			//ExportFileNameInfo.ClearAllNotifications();
			//if (ExportFileName.IsEmpty)
			//{
			//    ExportFileNameInfo.AddError("Please enter a File Name.");
			//}
		}

		ZString exportFileName;

		#endregion

		#region Implementation

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AdditionalSettingsRegistryBusinessObject();
		}

		protected override void RunPreSaveValidationCore()
		{
			ValidateExportFileName();
			base.RunPreSaveValidationCore();
		}

		#region Xml Serialisation

		protected override void WriteElements(XmlWriter writer)
		{
			base.WriteElements(writer);
			writer.WriteElementString(Schema.ExportFileName, ExportFileName.ToString());
		}

		protected override void ReadElements(XmlReaderWrapper reader)
		{
			base.ReadElements(reader);
			exportFileName = new ZString(reader.ReadElementString(Schema.ExportFileName));
		}

		public override ZDateTime LastRunDateTime
		{
			get
			{
				if (lastRunDateTime.IsEmpty)
				{
					if (NextRunDateTime.IsEmpty)
					{
						NextRunDateTime = ZDateTime.Now.AddHours(-2);
					}

					switch (IntervalType)
					{
						case "MONTHS": lastRunDateTime = NextRunDateTime.AddMonths(-Interval);
							break;
						case "DAYS": lastRunDateTime = NextRunDateTime.AddDays(-Interval);
							break;
						case "HOURS": lastRunDateTime = NextRunDateTime.AddHours(-Interval);
							break;
						case "MINUTES": lastRunDateTime = NextRunDateTime.AddMinutes(-Interval);
							break;
					}
				}
				return lastRunDateTime;
			}
			set
			{
				base.LastRunDateTime = value;
			}
		}

		#endregion

		#endregion
	}
}
