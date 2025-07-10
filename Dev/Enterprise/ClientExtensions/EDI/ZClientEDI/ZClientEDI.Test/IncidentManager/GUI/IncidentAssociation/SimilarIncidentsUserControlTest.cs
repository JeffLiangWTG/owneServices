using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Main.Navigation;
using CargoWiseOne.ResourceStrings;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Business.IncidentAssociation;
using Enterprise.Client.EDI.Modules;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	public class SimilarIncidentsUserControlTest : TestCaseWithFactory
	{
		public void TestSetDataBinding()
		{
			var incident = Factory.New<SupportIncident>();
			using (var control = new SimilarIncidentsUserControl())
			{
				control.SetDataBinding(incident, "");
				Assert(control.CurrentDataItem is SimilarIncidentsWrapper);
				control.SetDataBinding(null, "");
				AssertNull(control.CurrentDataItem);
				var dummy = Factory.New<DummyBusinessObject>();
				AssertExceptionThrown<ArgumentException>("Can bind only to SupportIncident parent object, but was " + dummy.GetType().FullName, () => control.SetDataBinding(dummy, ""));
			}
		}

		public void TestFillMenuItems()
		{
			var incident = Factory.New<SupportIncident>();
			var similarIncident = Factory.New<SupportIncident>();
			var incidentSimilarityMatrix = Factory.New<IncidentSimilarityMatrix>();
			incidentSimilarityMatrix.ISM_IM_Incident1 = incident.PK;
			incidentSimilarityMatrix.ISM_IM_Incident2 = similarIncident.PK;
			Factory.Save();

			using (var form = new ZForm(incident))
			{
				using (var control = new SimilarIncidentsUserControl())
				{
					form.Controls.Add(control);
					form.ControllerID = ClientControllerRegistration.SupportIncident;

					var caption = (NoResString)"Similar Incidents";
					var itemsViewModel = new MenuSection(caption.ToString(Res.CurrentLanguage), "Similar Incidents", caption, SectionType.RecentItem);

					var similarIncidentsWrapper = new SimilarIncidentsWrapper(incident);

					var similarIncidentsWrapperField = typeof(SimilarIncidentsUserControl).GetField("similarIncidentsWrapper", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					similarIncidentsWrapperField.SetValue(control, similarIncidentsWrapper);

					var options = new SimilarIncidentSearchOptions();
					options.SourceIncidentPK = incident.PK;
					options.FromTime = null;
					options.ToTime = DateTime.Now;
					var similarities = ObjectFactory.Get<ISimilarIncidentRepository>().SearchStoredIncidentSimilarities(options).ToList();
					similarities.FirstOrDefault()?.GetOtherIncident(incident.PK)?.Factory?.RelinquishThreadOwnership();

					var fillMenuItemsMethod = typeof(SimilarIncidentsUserControl).GetMethod("FillMenuItems", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
					var hasNewItems = (bool)fillMenuItemsMethod.Invoke(control, new object[] { itemsViewModel, similarities });

					Assert("Should has new items", hasNewItems);

					form.ControllerID = null;
					hasNewItems = (bool)fillMenuItemsMethod.Invoke(control, new object[] { itemsViewModel, similarities });
					Assert("Should not has new items", !hasNewItems);
				}
			}
		}
	}
}
