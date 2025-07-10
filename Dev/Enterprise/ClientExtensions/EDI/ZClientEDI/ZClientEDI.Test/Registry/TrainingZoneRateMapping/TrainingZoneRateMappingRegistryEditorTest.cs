using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.Client.EDI.Registry.Business;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.GUI;
using Enterprise.Registry.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Client.EDI.Registry.GUI.Testing
{
	[TestedType(typeof(TrainingZoneRateRegistryEditor))]
	public class TrainingZoneRateMappingRegistryEditorTest : RegistryItemEditorTestCase
	{
		#region Implementation
		protected override IRegistryItem GetRegistryItemWithSystemStorageLevel()
		{
			return new TrainingZoneRateRegistryItem("");
		}

		protected override RegistryItemEditor GetEditor()
		{
			return new TrainingZoneRateRegistryEditor(new TrainingZoneRateDataType(), null, Factory);
		}

		protected override Type GetExpectedEditorPaneType()
		{
			return typeof(TrainingZoneRateControl);
		}

		protected override object[] GetValidRegistryValues()
		{
			TrainingZoneRateCollection[] rates = new TrainingZoneRateCollection[1];
			RefZoneHeader zoneForTest = Factory.NewWithValidTestData<RefZoneHeader>();
			zoneForTest.FZ_ZoneType = EDIRefZoneHeaderLookups.EDIZoneTypeCodes.Training;
			Factory.Save();
			rates[0] = new TrainingZoneRateCollection();
			TrainingZoneRate rate = rates[0].AddNew();
			rate.ZonePK = zoneForTest.PK;
			rate.RateAmount = (ZDecimal)123m;
			rate.CurrencyCode = "AUD";
			return rates;
		}

		protected override RegistryItemEditor.EditorPaneAnchor ExpectedAnchor
		{
			get
			{
				return RegistryItemEditor.EditorPaneAnchor.TopLeft;
			}
		}

		protected override bool GetEditorPaneEnabledState(Control editorPane)
		{
			return !((TrainingZoneRateControl)editorPane).ReadOnly;
		}
		#endregion Implementation
	}
}
