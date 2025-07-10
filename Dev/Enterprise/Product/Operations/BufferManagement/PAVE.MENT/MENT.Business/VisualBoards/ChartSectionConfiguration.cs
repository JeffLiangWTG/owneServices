using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.PAVE.MENT.Business
{
	public class ChartSectionConfiguration : NonPersistentBusinessObject<ChartSectionConfigurationValidation>, IBoardSectionConfigurationBizo
	{
		public ChartSectionConfiguration(IBMBoardSection section)
			: base(section.Factory)
		{
			Argument.NotNull(section, nameof(section));
		}

		#region Xml Properties

		[XmlColumnProperty]
		[List("Extractions")]
		[ResourceStringData("ChartSectionConfiguration.ExtractionPK", Caption = "Extraction")]
		public ZGuid ExtractionPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(ExtractionPKInfo); }
			set
			{
				SetXmlColumnPropertyValue(ExtractionPKInfo, value);
				OverrideDefaultVisualisation = false;
				SectionNameInfo.RefreshBinding();
			}
		}

		public ZPropertyInfo ExtractionPKInfo
		{
			get { return GetZPropertyInfo(nameof(ExtractionPK)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("ChartSectionConfiguration.VisualisationPK", Caption = "Visualization")]
		public ZGuid VisualisationPK
		{
			get { return GetXmlColumnPropertyValue<ZGuid>(VisualisationPKInfo); }
			set
			{
				SetXmlColumnPropertyValue(VisualisationPKInfo, value);
				visualisation = null;
			}
		}

		public ZPropertyInfo VisualisationPKInfo
		{
			get { return GetZPropertyInfo(nameof(VisualisationPK)); }
		}

		[XmlColumnProperty]
		[ResourceStringData("ChartSectionConfiguration.OverrideDefaultVisualisation", Caption = "Override Default Visualization")]
		public ZBool OverrideDefaultVisualisation
		{
			get { return GetXmlColumnPropertyValue<ZBool>(OverrideDefaultVisualisationInfo); }
			set
			{
				if (value != OverrideDefaultVisualisation)
				{
					if (value)
					{
						CreateNewVisualisation();
					}
					else
					{
						DeleteVisualisationIfExists();
					}
				}

				SetXmlColumnPropertyValue(OverrideDefaultVisualisationInfo, value);
			}
		}

		void DeleteVisualisationIfExists()
		{
			var loadedVisualisation = Factory.Load<MENTAgedScoreVisualisation>(VisualisationPK);
			if (loadedVisualisation != null)
			{
				loadedVisualisation.Delete();
				VisualisationPK = ZGuid.Empty;
			}
		}

		void CreateNewVisualisation()
		{
			if (Extraction != null)
			{
				var newVisualisation = Extraction.CreateNewRelatedVisualisation();
				VisualisationPK = newVisualisation.PK;
			}
		}

		public ZPropertyInfo OverrideDefaultVisualisationInfo
		{
			get { return GetZPropertyInfo(nameof(OverrideDefaultVisualisation)); }
		}

		#endregion

		#region New Properties

		public MENTAgedScoreExtractionCollection Extractions
		{
			get { return new MENTAgedScoreExtractionCollection(Factory); }
		}

		public MENTAgedScoreExtraction Extraction
		{
			get { return Factory.Load<MENTAgedScoreExtraction>(ExtractionPK); }
		}

		[ChildEditable]
		MENTAgedScoreVisualisation Visualisation
		{
			get
			{
				if (visualisation == null)
				{
					visualisation = Factory.Load<MENTAgedScoreVisualisation>(VisualisationPK);
					RegisterEditableChildObject(visualisation);
				}

				return visualisation;
			}
		}

		MENTAgedScoreVisualisation visualisation;

		public MENTAgedScoreVisualisation RelatedVisualisation
		{
			get
			{
				return Extraction == null
					? null
					: OverrideDefaultVisualisation
						? Visualisation
						: Extraction.DefaultVisualisation;
			}
		}

		#endregion

		#region BusinessObject Overrides

		public override ChartSectionConfigurationValidation GetNewValidation()
		{
			return new ChartSectionConfigurationValidation(this);
		}

		#endregion

		#region IBoardSectionConfigurationBizo Members

		public ZString SectionName
		{
			get
			{
				if (RelatedVisualisation != null && !RelatedVisualisation.GraphTitle.IsEmpty)
				{
					return RelatedVisualisation.GraphTitle;
				}
				else if (Extraction != null)
				{
					return Extraction.MEX_Name;
				}
				return ZString.Empty;
			}
		}

		public ZPropertyInfo SectionNameInfo
		{
			get { return GetZPropertyInfo(nameof(SectionName)); }
		}

		public void CopyConfigurationPropertiesToNewSection(IBMBoardSection section)
		{
			var cloneSection = (BMBoardSection)section;
			var cloneConfig = cloneSection.Configuration as ChartSectionConfiguration;

			// create new visualisation on cloning, otherwise modifying cloned visualisation will modify original visualisation
			if (cloneConfig != null && cloneConfig.OverrideDefaultVisualisation)
			{
				cloneConfig.CreateNewVisualisation();
				Visualisation.CopyXmlColumns(cloneConfig.Visualisation);
			}
		}

		#endregion
	}
}
