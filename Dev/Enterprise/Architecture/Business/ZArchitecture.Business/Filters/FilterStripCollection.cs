using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.ZArchitecture.Business.Internal
{
	[XmlSerializerAssembly("Enterprise.ZArchitecture.Business.XmlSerializers")]
	public class FilterStripCollection : NonPersistentBusinessObjectCollection<FilterStrip>
	{
		#region Construction

		public FilterStripCollection(ModuleFilterCollection moduleFilters)
		{
			if (moduleFilters == null)
			{
				throw new ArgumentNullException(nameof(moduleFilters), "Cannot create a FilterStripCollection with a null ModuleFilterCollection.");
			}
			ModuleFilters = moduleFilters;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FilterStrip(ModuleFilters);
		}

		readonly ModuleFilterCollection ModuleFilters;

		#endregion

		#region ClearValues

		public void ClearValues()
		{
			foreach (FilterStrip strip in this)
			{
				if (strip.CurrentModuleFilter != null)
				{
					strip.CurrentModuleFilter.Clear();
				}
			}
		}

		#endregion

		#region AddNew

		public FilterStrip AddNew(ZString filterDescription)
		{
			var result = AddNew();
			result.FilterDescription = filterDescription;
			result.ModuleFilterChanged += new EventHandler(result_ModuleFilterChanged);
			return result;
		}

		public event EventHandler ModuleFilterChanged;

		void result_ModuleFilterChanged(object sender, EventArgs e)
		{
			if (ModuleFilterChanged != null)
			{
				ModuleFilterChanged(this, EventArgs.Empty);
			}
		}
		public FilterStrip AddNew(ModuleFilter moduleFilter)
		{
			var strip = AddNew();
			strip.OrCategory = moduleFilter.OrCategory;
			strip.IncludeDuplicatesWhenGettingModuleFilter = moduleFilter.IsDuplicateDefault;
			strip.FilterDescription = moduleFilter.Description;

			if (moduleFilter.IsDuplicateDefault)
			{
				using (strip.SuspendTriggeringChangeOfModuleFilterOnSettingDescription())
				{
					strip.FilterDescription = moduleFilter.DescriptionWithoutInstanceNumber;
				}
			}

			return strip;
		}

		#endregion

		#region LoadFromXml

		public void LoadFromXml(StmModuleFilter layout, StmModuleFilterUserData layoutUserData)
		{
			using (var layoutStream = new MemoryStream(layout.S9_FilterData))
			using (var layoutReader = new XmlTextReader(layoutStream))
			using (var dataStream = layoutUserData != null && !layoutUserData.S0_FilterDataValues.IsEmpty ? new MemoryStream(layoutUserData.S0_FilterDataValues) : null)
			using (var dataReader = dataStream != null ? new XmlTextReader(dataStream) : null)
			{
				layoutReader.WhitespaceHandling = WhitespaceHandling.None;
				if (dataReader != null)
				{
					dataReader.WhitespaceHandling = WhitespaceHandling.None;
				}

				LoadFromXml(layoutReader, dataReader);
			}
		}

		public void LoadFromXml(XmlReader layoutReader, XmlReader dataReader, Action prepareSecondReader = null, bool showMissingValusMessage = true)
		{
			var failedStrips = new List<string>();

			SuspendValidation();
			try
			{
				RemoveAll();

				// read layout
				((IXmlSerializable)LayoutSerializer).ReadXml(layoutReader);

				// read values
				if (dataReader != null)
				{
					if (prepareSecondReader != null)
					{
						prepareSecondReader();
					}

					((IXmlSerializable)LayoutValuesSerializer).ReadXml(dataReader);
				}

				foreach (FilterStrip strip in this)
				{
					var moduleFilter = strip.CurrentModuleFilter;
					if (moduleFilter != null)
					{
						failedStrips.AddRange(moduleFilter.FailedFilterStrips);
					}
				}
			}
			finally
			{
				ResumeValidation();
			}

			if (failedStrips.Count > 0 && showMissingValusMessage)
			{
				var caption = Res.GetString("db7d6cf6-8335-41b5-a15e-3b237f22c266", "Error during loading of filter strips");
				var message = Res.GetString("dcc27f95-47c1-4bfb-a88c-3282ac4af937", "The following filter strips failed to load")
					+ ":\r\n"
					+ string.Join(System.Environment.NewLine, failedStrips.ToArray())
					+ "\r\n\r\n"
					+ Res.GetString("49341303-e409-443f-88a9-c976d4d04023", "These filters will no longer have any values you may have assigned to them.\r\nPlease enter their values before searching.");
				Globals.Message.ShowError(message, caption);
			}
		}

		#endregion

		#region GetLayoutAsXml / GetLayoutValuesAsXml

		public ZBlob GetLayoutAsXml()
		{
			using (var stream = new MemoryStream())
			{
				WriteLayoutToXml(stream);
				return stream.ToArray();
			}
		}

		public void WriteLayoutToXml(Stream stream)
		{
			var serializer = ZXmlSerializer.New(LayoutSerializer.GetType());
			serializer.Serialize(stream, LayoutSerializer);
		}

		public ZBlob GetLayoutValuesAsXml()
		{
			using (var stream = new MemoryStream())
			{
				WriteLayoutValuesToXml(stream);
				return stream.ToArray();
			}
		}

		public void WriteLayoutValuesToXml(Stream stream)
		{
			var serializer = ZXmlSerializer.New(LayoutValuesSerializer.GetType());
			serializer.Serialize(stream, LayoutValuesSerializer);
		}

		#endregion

		#region Serialization objects

		FilterLayoutSerializer LayoutSerializer
		{
			get { return fLayoutSerializer ?? (fLayoutSerializer = new FilterLayoutSerializer(this)); }
		}

		FilterLayoutValuesSerializer LayoutValuesSerializer
		{
			get { return fLayoutValuesSerializer ?? (fLayoutValuesSerializer = new FilterLayoutValuesSerializer(this)); }
		}

		FilterLayoutSerializer fLayoutSerializer;
		FilterLayoutValuesSerializer fLayoutValuesSerializer;

		#endregion

		#region class FilterStripsSerializer

		[XmlSerializerAssembly("Enterprise.ZArchitecture.Business.XmlSerializers")]
		public abstract class FilterStripsSerializer : IXmlSerializable
		{
			public FilterStripsSerializer(FilterStripCollection collection)
			{
				Collection = collection;
			}

			protected FilterStripsSerializer()
			{
			}

			protected readonly FilterStripCollection Collection;

			protected abstract void DeserializeXml(XmlReader reader);
			protected abstract void SerializeXml(XmlWriter writer);

			#region IXmlSerializable Members

			public XmlSchema GetSchema()
			{
				return null;
			}

			public void ReadXml(XmlReader reader)
			{
				DeserializeXml(reader);
			}

			public void WriteXml(XmlWriter writer)
			{
				SerializeXml(writer);
			}

			#endregion
		}

		#endregion

		#region class FilterLayoutSerializer

		[XmlSerializerAssembly("Enterprise.ZArchitecture.Business.XmlSerializers")]
		public class FilterLayoutSerializer : FilterStripsSerializer
		{
			public FilterLayoutSerializer(FilterStripCollection collection)
				: base(collection)
			{
			}

			[EditorBrowsable(EditorBrowsableState.Never)] // XmlSerializer will blow up without this
			FilterLayoutSerializer()
			{
			}

			protected override void DeserializeXml(XmlReader reader)
			{
				reader.ReadToFollowing("FilterStrip");

				while (reader.IsStartElement("FilterStrip"))
				{
					reader.ReadStartElement("FilterStrip");
					if (!reader.IsEmptyElement)
					{
						ZString description = reader.ReadElementString("FilterDescription");
						if (!description.IsEmpty)
						{
							var strip = Collection.AddNew(description);
							var orCategory = FilterOrCategory.None;
							if (reader.IsStartElement("OrCategory"))
							{
								var category = reader.ReadElementString("OrCategory");
								if (!string.IsNullOrEmpty(category) && Enum.IsDefined(typeof(FilterOrCategory), category))
								{
									strip.OrCategory = (FilterOrCategory)Enum.Parse(typeof(FilterOrCategory), category);
									orCategory = strip.OrCategory;
								}
							}

							if (reader.IsStartElement("GroupOrCategory"))
							{
								var groupOrCategory = reader.ReadElementString("GroupOrCategory");
								if (!string.IsNullOrEmpty(groupOrCategory) && Enum.IsDefined(typeof(FilterOrCategory), groupOrCategory))
								{
									strip.GroupOrCategory = (FilterOrCategory)Enum.Parse(typeof(FilterOrCategory), groupOrCategory);
								}
							}

							if (reader.IsStartElement("GroupName"))
							{
								var groupName = reader.ReadElementString("GroupName");
								if (!string.IsNullOrEmpty(groupName))
								{
									strip.GroupName = groupName;
									strip.OrCategory = orCategory;//set group name changes OrCategory to it's parent OrCategory. We set it back here. 
								}
							}

							if (reader.IsStartElement("AdditionalColourName"))
							{
								var additionalColorName = reader.ReadElementString("AdditionalColourName");
								if (!string.IsNullOrEmpty(additionalColorName))
								{
									strip.AdditionalColourName = additionalColorName;
								}
							}

							if (reader.IsStartElement("AdditionalGroupColourName"))
							{
								var additionalGroupColorName = reader.ReadElementString("AdditionalGroupColourName");
								if (!string.IsNullOrEmpty(additionalGroupColorName))
								{
									strip.AdditionalGroupColourName = additionalGroupColorName;
								}
							}

							if (reader.IsStartElement("FilterPropertyLockStatus"))
							{
								var filterPropertyLockStatus = reader.ReadElementString("FilterPropertyLockStatus");

								if (!string.IsNullOrEmpty(filterPropertyLockStatus))
								{
									strip.FilterPropertyLockStatus = bool.Parse(filterPropertyLockStatus);
								}
							}
						}
					}

					if (!reader.IsStartElement("FilterStrip"))
					{
						reader.ReadToFollowing("FilterStrip");
					}
				}
			}

			protected override void SerializeXml(XmlWriter writer)
			{
				writer.WriteStartElement("FilterStrips");

				foreach (FilterStrip strip in Collection)
				{
					if (strip.CurrentModuleFilter != null)
					{
						writer.WriteStartElement("FilterStrip");
						writer.WriteElementString("FilterDescription", strip.FilterDescription);
						writer.WriteElementString("OrCategory", strip.OrCategory.ToString());
						writer.WriteElementString("GroupOrCategory", strip.GroupOrCategory.ToString());
						writer.WriteElementString("GroupName", strip.GroupName);
						writer.WriteElementString("AdditionalColourName", strip.AdditionalColourName);
						writer.WriteElementString("AdditionalGroupColourName", strip.AdditionalGroupColourName);
						writer.WriteElementString("FilterPropertyLockStatus", strip.FilterPropertyLockStatus.ToString());
						writer.WriteEndElement();
					}
				}

				writer.WriteEndElement();
			}
		}

		#endregion

		#region class FilterLayoutValuesSerializer

		[XmlSerializerAssembly("Enterprise.ZArchitecture.Business.XmlSerializers")]
		public class FilterLayoutValuesSerializer : FilterStripsSerializer
		{
			public FilterLayoutValuesSerializer(FilterStripCollection collection)
				: base(collection)
			{
			}

			[EditorBrowsable(EditorBrowsableState.Never)] // XmlSerializer will blow up without this
			FilterLayoutValuesSerializer()
			{
			}

			protected override void DeserializeXml(XmlReader reader)
			{
				foreach (IXmlSerializable strip in Collection)
				{
					strip.ReadXml(reader);
				}
			}

			protected override void SerializeXml(XmlWriter writer)
			{
				writer.WriteStartElement("ModuleFilters");

				foreach (IXmlSerializable strip in Collection)
				{
					strip.WriteXml(writer);
				}

				writer.WriteEndElement();
			}
		}

		#endregion
	}
}
