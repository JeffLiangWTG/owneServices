using System;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DataTransfer.Native.ConcreteUnitTesting
{
	class BMControlCustomisationTest : TestCaseWithFactory
	{
		public void TestBMControlCustomisation()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_Customisation)))
			{
				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(stream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: BMControlCustomisation
--- Import Process Finished -----------------------------------------------------------
BMControlCustomisation - 1 inserts, 0 updates, 0 deletes
						".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(BMControlCustomisationSchema.FM_Name, "Container smaller thingy");

				var factory = new BusinessObjectFactory();
				var loadedCustomisation = factory.Load<BMControlCustomisation>(query).Single();

				AssertEquals("Container smaller thingy", loadedCustomisation.FM_Name);
				AssertEquals(3, loadedCustomisation.CustomisationLines.Count);
				AssertEquals("ProviderJobNumber", loadedCustomisation.CustomisationLines[0].PropertyName);
				AssertEquals("RelevantEstimateHoursLabel", loadedCustomisation.CustomisationLines[1].PropertyName);
				AssertEquals("P9_Description", loadedCustomisation.CustomisationLines[2].PropertyName);

				using (var baseStream = NativeDataTransferTestHelper.ExportToStream(loadedCustomisation))
				{
					StreamReader reader = new StreamReader(baseStream, Encoding.UTF8);
					string text = reader.ReadToEnd();
					AssertRelevantLinesMatch(XML_Customisation, text);
				}
			}
		}

		public void TestImportBMControlCustomisation_SpecialCharactersInLayoutData()
		{
			using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(XML_Customisation_SpecialChars)))
			{
				var insertLog = NativeDataTransferTestHelper.ImportAndGetInsertLog(stream);
				AssertMultilineASCIIEquals("Insert Log Text", @"
--- Start Import Process --------------------------------------------------------------
Processed: BMControlCustomisation
--- Import Process Finished -----------------------------------------------------------
BMControlCustomisation - 1 inserts, 0 updates, 0 deletes
						".Trim(), insertLog);

				var query = new ZQuery();
				query.AddToFilter(BMControlCustomisationSchema.FM_Name, "Container thingy");

				var factory = new BusinessObjectFactory();
				var loadedCustomisation = factory.Load<BMControlCustomisation>(query).Single();

				using (var baseStream = NativeDataTransferTestHelper.ExportToStream(loadedCustomisation))
				{
					StreamReader reader = new StreamReader(baseStream, Encoding.UTF8);
					string text = reader.ReadToEnd();
					AssertRelevantLinesMatch(XML_Customisation_SpecialChars, text);
				}
			}
		}

		void AssertRelevantLinesMatch(string text1, string text2)
		{
			string[] keys = { "JobType", "Name", "ControlType", "LayoutData" };
			var lines1 = text1.Split('\n').Select(s => s.Trim()).Where(s => keys.Any(s.Contains)).ToList();
			var lines2 = text2.Split('\n').Select(s => s.Trim()).Where(s => keys.Any(s.Contains)).ToList();
			lines1.Sort();
			lines2.Sort();
			AssertEquals(string.Join("\n", lines1), string.Join("\n", lines2));
		}

		static string XML_Customisation => FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>WTGCUSBNE</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <BMControlCustomisation version=""2.0"">
      <BMControlCustomisation Action=""MERGE"">
        <PK>e4aae275-6a91-4d74-9eb8-6896efe04ff9</PK>
        <JobType>CNT</JobType>
        <Name>Container smaller thingy</Name>
        <ControlType>TAS</ControlType>
        <IsSystemWide>false</IsSystemWide>
        <LayoutData>&lt;FM_LayoutData&gt;&lt;Width&gt;122&lt;/Width&gt;&lt;Height&gt;57&lt;/Height&gt;&lt;BackgroundColor&gt;White&lt;/BackgroundColor&gt;&lt;CustomisationLines&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;200&lt;/Width&gt;&lt;PropertySource&gt;WFL&lt;/PropertySource&gt;&lt;PropertyName&gt;ProviderJobNumber&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;AutoSize&gt;Y&lt;/AutoSize&gt;&lt;Left&gt;2&lt;/Left&gt;&lt;Top&gt;2&lt;/Top&gt;&lt;Height&gt;16&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsBold&gt;Y&lt;/IsBold&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;200&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;RelevantEstimateHoursLabel&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;AutoSize&gt;Y&lt;/AutoSize&gt;&lt;Left&gt;82&lt;/Left&gt;&lt;Top&gt;2&lt;/Top&gt;&lt;Height&gt;16&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;RIGHT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;200&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_Description&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;AutoSize&gt;Y&lt;/AutoSize&gt;&lt;Left&gt;2&lt;/Left&gt;&lt;Top&gt;34&lt;/Top&gt;&lt;Height&gt;16&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;/CustomisationLines&gt;&lt;CustomisedControls&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;12&lt;/Width&gt;&lt;ControlType&gt;TSI&lt;/ControlType&gt;&lt;Left&gt;110&lt;/Left&gt;&lt;Top&gt;2&lt;/Top&gt;&lt;Height&gt;12&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;RIGHT&lt;/Alignment&gt;&lt;Orientation&gt;Vertical&lt;/Orientation&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;120&lt;/Width&gt;&lt;ControlType&gt;TAG&lt;/ControlType&gt;&lt;Left&gt;1&lt;/Left&gt;&lt;Top&gt;48&lt;/Top&gt;&lt;Height&gt;11&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;/CustomisedControls&gt;&lt;/FM_LayoutData&gt;</LayoutData>
        <SystemCreateTimeUtc>2018-06-29T02:24:33</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2019-06-05T13:04:22</SystemLastEditTimeUtc>
      </BMControlCustomisation>
    </BMControlCustomisation>
  </Body>
</Native>");

		static string XML_Customisation_SpecialChars => FormattableString.Invariant($@"<?xml version=""1.0"" encoding=""utf-8""?>
<Native xmlns=""http://www.cargowise.com/Schemas/Native/2011/11"" version=""2.0"">
  <Header>
    <OwnerCode>WTGCUSBNE</OwnerCode>
    <EnableCodeMapping>true</EnableCodeMapping>
  </Header>
  <Body>
    <BMControlCustomisation version=""2.0"">
      <BMControlCustomisation Action=""MERGE"">
        <PK>3051774F-0217-41F3-939B-A0358984C55E</PK>
        <JobType>CNT</JobType>
        <Name>Container thingy</Name>
        <ControlType>DET</ControlType>
        <IsSystemWide>false</IsSystemWide>
        <LayoutData>&lt;FM_LayoutData&gt;&lt;Width&gt;350&lt;/Width&gt;&lt;Height&gt;238&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;CustomisationLines&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;80&lt;/Width&gt;&lt;PropertySource&gt;WFL&lt;/PropertySource&gt;&lt;PropertyName&gt;ProviderJobNumber&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsBold&gt;Y&lt;/IsBold&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;235&lt;/Width&gt;&lt;PropertySource&gt;WFL&lt;/PropertySource&gt;&lt;PropertyName&gt;ProviderJobDescription&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;235&lt;/Width&gt;&lt;PropertySource&gt;WFL&lt;/PropertySource&gt;&lt;PropertyName&gt;FH_CompletionStatement&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;20&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;250&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_Description&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Label&gt;Task&lt;/Label&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;43&lt;/Top&gt;&lt;Height&gt;16&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;300&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;CardStatusDescription&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;61&lt;/Top&gt;&lt;Height&gt;18&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;350&lt;/Width&gt;&lt;PropertySource&gt;JOB&lt;/PropertySource&gt;&lt;PropertyName&gt;&amp;lt;JobContainer+JC_FCLAvailableInfo.Notifications&amp;gt;&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;76&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;113&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;EstimatedHandoverTimeLocal&lt;/PropertyName&gt;&lt;ControlType&gt;TIM&lt;/ControlType&gt;&lt;Label&gt;Estimated Handover Time&lt;/Label&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;93&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;90&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_EstimatedTimeToComplete&lt;/PropertyName&gt;&lt;ControlType&gt;DUR&lt;/ControlType&gt;&lt;Label&gt;Time Left (ETC)&lt;/Label&gt;&lt;Left&gt;290&lt;/Left&gt;&lt;Top&gt;93&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;90&lt;/Width&gt;&lt;PropertySource&gt;WFL&lt;/PropertySource&gt;&lt;PropertyName&gt;AgreedDeliveryDateLocal&lt;/PropertyName&gt;&lt;ControlType&gt;TIM&lt;/ControlType&gt;&lt;Label&gt;Delivery Date&lt;/Label&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;118&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;IsReadOnly&gt;Y&lt;/IsReadOnly&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;250&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_CardNote&lt;/PropertyName&gt;&lt;ControlType&gt;TXT&lt;/ControlType&gt;&lt;Label&gt;Card Note&lt;/Label&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;144&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Light Yellow&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;30&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_IsCalendarItem&lt;/PropertyName&gt;&lt;ControlType&gt;FLG&lt;/ControlType&gt;&lt;Label&gt;Set Reminder&lt;/Label&gt;&lt;Top&gt;170&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;Width&gt;113&lt;/Width&gt;&lt;PropertySource&gt;TSK&lt;/PropertySource&gt;&lt;PropertyName&gt;P9_ScheduledDate&lt;/PropertyName&gt;&lt;ControlType&gt;TIM&lt;/ControlType&gt;&lt;Label&gt;.&lt;/Label&gt;&lt;Left&gt;85&lt;/Left&gt;&lt;Top&gt;170&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.BMControlCustomisationLine&gt;&lt;/CustomisationLines&gt;&lt;CustomisedControls&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;100&lt;/Width&gt;&lt;ControlType&gt;CAP&lt;/ControlType&gt;&lt;Left&gt;221&lt;/Left&gt;&lt;Top&gt;2&lt;/Top&gt;&lt;Height&gt;25&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;25&lt;/Width&gt;&lt;ControlType&gt;CLS&lt;/ControlType&gt;&lt;Left&gt;323&lt;/Left&gt;&lt;Top&gt;2&lt;/Top&gt;&lt;Height&gt;25&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;12&lt;/Width&gt;&lt;ControlType&gt;TSI&lt;/ControlType&gt;&lt;Left&gt;70&lt;/Left&gt;&lt;Top&gt;64&lt;/Top&gt;&lt;Height&gt;12&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;RIGHT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;84&lt;/Width&gt;&lt;ControlType&gt;NDG&lt;/ControlType&gt;&lt;Left&gt;205&lt;/Left&gt;&lt;Top&gt;115&lt;/Top&gt;&lt;Height&gt;26&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;58&lt;/Width&gt;&lt;ControlType&gt;DAP&lt;/ControlType&gt;&lt;Left&gt;290&lt;/Left&gt;&lt;Top&gt;115&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;200&lt;/Width&gt;&lt;ControlType&gt;CDL&lt;/ControlType&gt;&lt;Left&gt;205&lt;/Left&gt;&lt;Top&gt;170&lt;/Top&gt;&lt;Height&gt;20&lt;/Height&gt;&lt;BackgroundColor&gt;Gold&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;352&lt;/Width&gt;&lt;ControlType&gt;TAG&lt;/ControlType&gt;&lt;Top&gt;196&lt;/Top&gt;&lt;Height&gt;15&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;BringToFront&gt;Y&lt;/BringToFront&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;211&lt;/Width&gt;&lt;ControlType&gt;STS&lt;/ControlType&gt;&lt;Left&gt;20&lt;/Left&gt;&lt;Top&gt;214&lt;/Top&gt;&lt;Height&gt;26&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;61&lt;/Width&gt;&lt;ControlType&gt;JOB&lt;/ControlType&gt;&lt;Label&gt;Open Job&lt;/Label&gt;&lt;Left&gt;240&lt;/Left&gt;&lt;Top&gt;214&lt;/Top&gt;&lt;Height&gt;22&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;Width&gt;40&lt;/Width&gt;&lt;ControlType&gt;SAV&lt;/ControlType&gt;&lt;Label&gt;Save&lt;/Label&gt;&lt;Left&gt;305&lt;/Left&gt;&lt;Top&gt;214&lt;/Top&gt;&lt;Height&gt;22&lt;/Height&gt;&lt;BackgroundColor&gt;Transparent&lt;/BackgroundColor&gt;&lt;ForegroundColor&gt;Black&lt;/ForegroundColor&gt;&lt;FontSize&gt;8&lt;/FontSize&gt;&lt;Alignment&gt;LEFT&lt;/Alignment&gt;&lt;/Enterprise.BufferManagement.Business.StaticControlCustomisation&gt;&lt;/CustomisedControls&gt;&lt;/FM_LayoutData&gt;</LayoutData>
        <SystemCreateTimeUtc>2018-06-29T01:21:07</SystemCreateTimeUtc>
        <SystemLastEditTimeUtc>2018-06-29T01:58:18</SystemLastEditTimeUtc>
      </BMControlCustomisation>
    </BMControlCustomisation>
  </Body>
</Native>");
	}
}
