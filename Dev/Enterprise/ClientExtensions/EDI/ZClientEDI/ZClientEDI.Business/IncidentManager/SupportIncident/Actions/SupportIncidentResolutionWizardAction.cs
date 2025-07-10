using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Xml;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class SupportIncidentResolutionWizardAction : SupportIncidentAction, IXmlSerializable
	{
		public SupportIncidentResolutionWizardAction(SupportIncident incident)
			: base(incident)
		{
		}

		public SupportIncidentResolutionWizardAction() : base()
		{
		}

		public new BusinessObjectFactory Factory => Incident.Factory;

		public void SetIncident(SupportIncident incident) => Incident = incident;

		XmlSchema IXmlSerializable.GetSchema() => null;

		protected override void PerformAction()
		{
			dataStorage();

			AddInternalMessageAndCloseIncident();

			if (CompletelySolvedOption)
			{
				Incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials, Comment);
				AddMiscellaneousEvent(IncidentConstants.LogFreeText.CR5ResolutionAssistantResult, IncidentConstants.CR5ContentAvailable.CompletelySolved, "");
			}

			if ((PartlySolvedOption || ContentCouldNotBeFoundOption) && ContentIsIrrelevantOption)
			{
				Incident.CloseIncident(ResolutionMethod, Comment);
			}

			if (PartlySolvedOption || ContentCouldNotBeFoundOption)
			{
				var priority = DeterminePriority();
				if (PartlySolvedOption)
				{
					AddMiscellaneousEvent(IncidentConstants.LogFreeText.CR5ResolutionAssistantResult, IncidentConstants.CR5ContentAvailable.PartlySolved, priority);
				}
				else
				{
					AddMiscellaneousEvent(IncidentConstants.LogFreeText.CR5ResolutionAssistantResult, IncidentConstants.CR5ContentAvailable.ContentNotBeFound, priority);
				}
			}
		}

		string DeterminePriority()
		{
			if (ContentIsIrrelevantOption)
			{
				return IncidentConstants.CR5ContentPriority.DoNotDevelop;
			}
			else if (ContentBeDevelopNotWorthOption)
			{
				return IncidentConstants.CR5ContentPriority.LowPriority;
			}
			else
			{
				return IncidentConstants.CR5ContentPriority.NormalPriority;
			}
		}

		void AddMiscellaneousEvent(string miscellaneousEventDescription, string contentAvailable, string priority)
		{
			var miscellaneousEventParamList = new List<KeyValuePair<string, string>>();
			miscellaneousEventParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, miscellaneousEventDescription));
			miscellaneousEventParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ContentAvailable, contentAvailable));
			miscellaneousEventParamList.Add(new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Priority, priority));

			Incident.Logs.AddNew(
				AutoEvents.MiscellaneousEvent,
				miscellaneousEventParamList.ToArray()
			);
		}

		void dataStorage()
		{
			var serializer = ZXmlSerializer.New(typeof(SupportIncidentResolutionWizardAction));
			var writer = new StringWriter();
			serializer.Serialize(writer, this);
			var actualXml = writer.GetStringBuilder().ToString();

			Incident.ResolutionWizardOptionText = actualXml;
		}

		void AddInternalMessageAndCloseIncident()
		{
			if ((PartlySolvedOption || ContentCouldNotBeFoundOption) && (ContentBeDevelopYesOption || ContentBeDevelopNotWorthOption))
			{
				var sb = new StringBuilder();
				sb.AppendLine();
				sb.AppendLine("Content Escalation Notes");
				sb.AppendLine("~~~~~~~~~~~~~~~~~~~~~~~~~");
				sb.AppendLine();
				sb.AppendLine("** Result:");
				sb.AppendLine(PartlySolvedOption ? "Available content PARTLY solved the client query" : "Content could not be found");
				sb.AppendLine();

				sb.AppendLine("** Should content be developed?:");
				sb.AppendLine(ContentBeDevelopYesOption ? "Yes" : "Probably not worth it");
				sb.AppendLine();

				if (ContentBeDevelopNotWorthOption)
				{
					sb.AppendLine("** Reason if probably not worth it:");
					if (HighlyClientSpecificOption)
					{
						sb.AppendLine("Highly client specific / not broadly useful");
					}
					else if (ComplexEdgeCaseOption)
					{
						sb.AppendLine("Complex edge case");
					}
					else
					{
						sb.AppendLine(ReasonText);
					}
					sb.AppendLine();
				}

				if (PartlySolvedOption)
				{
					sb.AppendLine("** Links to existing Content:");
					sb.AppendLine(LinksToExistingContent);
					sb.AppendLine();
				}

				if (ContentBeDevelopYesOption)
				{
					sb.AppendLine("** What changes do you recommend to training content?");
					sb.AppendLine(RecommendToContentText);
					sb.AppendLine();
					sb.AppendLine("** How can we make this content easier for the next customer to find?");
					sb.AppendLine(ContentEasierText);
				}
				var latestNotes = Incident.EConversation.GetTimeOrderedMessages().FirstOrDefault(msg => msg.Body.Contains("Content Escalation Notes"));
				if (latestNotes == null || latestNotes.Body != sb.ToString())
				{
					Incident.AddInternalMessage(sb.ToString());
				}

				Incident.CloseIncident(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment, Comment);
			}
		}

		#region Comment

		public override ZString Comment
		{
			get => base.Comment;
			set => base.Comment = value;
		}
		#endregion

		#region Resolution Method

		[MaxLength(SupportIncident.Schema.IM_ResolutionCodeMaxLength)]
		public ZString ResolutionMethod
		{
			get { return resolutionMethod; }
			set
			{
				if (resolutionMethod != value)
				{
					CheckMaximumLength(ResolutionMethodInfo, value);
					resolutionMethod = value;
					ResolutionMethodInfo.RefreshBinding();
				}
			}
		}
		ZString resolutionMethod;

		public ZPropertyInfo ResolutionMethodInfo
		{
			get { return GetZPropertyInfo(nameof(ResolutionMethod)); }
		}

		public CodeDescriptionPairList ActiveCloseStatusDispositionList
		{
			get
			{
				return GetCloseStatusDispositionList();
			}
		}

		public CodeDescriptionPairList GetCloseStatusDispositionList(bool activeOnly = true)
		{
			return Factory.GetCachedValue(
				"SupportIncidentResolutionWizardAction.GetCloseStatusDispositionList:" + Incident.IM_Category + ":" + Incident.IM_Priority + ":" + Incident.IM_Product + ";" + activeOnly.ToString(),
				() =>
				{
					var tree = EDIDataRegistry.Instance.IncidentClosureDispositions.Value;
					var result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, Incident.IM_Priority, Incident.IM_Product);

					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, Incident.IM_Priority, CodeDescriptionBoolTreeNode.AllCode);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, CodeDescriptionBoolTreeNode.AllCode, Incident.IM_Product);
					}
					if (result.Count == 0)
					{
						result = tree.GetChildrenExactMatchOnly(activeOnly, Incident.IM_Category, CodeDescriptionBoolTreeNode.AllCode, CodeDescriptionBoolTreeNode.AllCode);
					}
					result.RemoveCode(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingReferredToLearningMaterials);
					result.RemoveCode(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingFlaggedForContentDevelopment);
					result.RemoveCode(SupportIncidentLookups.DispositionList.Constants.Closed.TrainingNoLearningMaterials);
					return result;
				});
		}

		#endregion

		#region Please choose from the following options:

		public ZBool CompletelySolvedOption
		{
			get { return completelySolvedOption; }
			set
			{
				completelySolvedOption = value;
				CompletelySolvedOptionInfo.RefreshBinding();
			}
		}

		ZBool completelySolvedOption;

		public ZPropertyInfo CompletelySolvedOptionInfo
		{
			get { return GetZPropertyInfo(nameof(CompletelySolvedOption)); }
		}

		public ZBool PartlySolvedOption
		{
			get { return partlySolvedOption; }
			set
			{
				partlySolvedOption = value;
				PartlySolvedOptionOptionInfo.RefreshBinding();
			}
		}

		ZBool partlySolvedOption;

		public ZPropertyInfo PartlySolvedOptionOptionInfo
		{
			get { return GetZPropertyInfo(nameof(PartlySolvedOption)); }
		}

		public ZBool ContentCouldNotBeFoundOption
		{
			get { return contentCouldNotBeFoundOption; }
			set
			{
				contentCouldNotBeFoundOption = value;
				ContentCouldNotBeFoundOptionInfo.RefreshBinding();
			}
		}

		ZBool contentCouldNotBeFoundOption;

		public ZPropertyInfo ContentCouldNotBeFoundOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ContentCouldNotBeFoundOption)); }
		}

		#endregion

		#region SHould content be developed, or its discoverability enhanced?

		public ZBool ContentBeDevelopYesOption
		{
			get { return contentBeDevelopYesOption; }
			set
			{
				contentBeDevelopYesOption = value;
				ContentBeDevelopYesOptionInfo.RefreshBinding();
			}
		}

		ZBool contentBeDevelopYesOption;

		public ZPropertyInfo ContentBeDevelopYesOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ContentBeDevelopYesOption)); }
		}

		public ZBool ContentBeDevelopNotWorthOption
		{
			get { return contentBeDevelopNotWorthOption; }
			set
			{
				contentBeDevelopNotWorthOption = value;
				ContentBeDevelopNotWorthOptionInfo.RefreshBinding();
			}
		}

		ZBool contentBeDevelopNotWorthOption;

		public ZPropertyInfo ContentBeDevelopNotWorthOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ContentBeDevelopNotWorthOption)); }
		}

		public ZBool ContentIsIrrelevantOption
		{
			get { return contentIsIrrelevantOption; }
			set
			{
				contentIsIrrelevantOption = value;
				ContentIsIrrelevantOptionInfo.RefreshBinding();
			}
		}

		ZBool contentIsIrrelevantOption;

		public ZPropertyInfo ContentIsIrrelevantOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ContentIsIrrelevantOption)); }
		}

		#endregion

		#region provide link

		public ZString LinksToExistingContent
		{
			get { return linksToExistingContent; }
			set
			{
				linksToExistingContent = value;
				LinksToExistingContentInfo.RefreshBinding();
			}
		}

		ZString linksToExistingContent;

		public ZPropertyInfo LinksToExistingContentInfo
		{
			get { return GetZPropertyInfo(nameof(LinksToExistingContent)); }
		}

		#endregion

		#region provide a reason

		public ZBool HighlyClientSpecificOption
		{
			get { return highlyClientSpecificOption; }
			set
			{
				highlyClientSpecificOption = value;
				HighlyClientSpecificOptionInfo.RefreshBinding();
			}
		}

		ZBool highlyClientSpecificOption;

		public ZPropertyInfo HighlyClientSpecificOptionInfo
		{
			get { return GetZPropertyInfo(nameof(HighlyClientSpecificOption)); }
		}

		public ZBool ComplexEdgeCaseOption
		{
			get { return complexEdgeCaseOption; }
			set
			{
				complexEdgeCaseOption = value;
				ComplexEdgeCaseOptionInfo.RefreshBinding();
			}
		}

		ZBool complexEdgeCaseOption;

		public ZPropertyInfo ComplexEdgeCaseOptionInfo
		{
			get { return GetZPropertyInfo(nameof(ComplexEdgeCaseOption)); }
		}

		public ZBool OtherOption
		{
			get { return otherOption; }
			set
			{
				otherOption = value;
				OtherOptionInfo.RefreshBinding();
			}
		}

		ZBool otherOption;

		public ZPropertyInfo OtherOptionInfo
		{
			get { return GetZPropertyInfo(nameof(OtherOption)); }
		}

		public ZString ReasonText
		{
			get { return reasonText; }
			set
			{
				reasonText = value;
				ReasonTextInfo.RefreshBinding();
			}
		}

		public bool ReasonText_ReadOnly => !OtherOption;

		ZString reasonText;

		public ZPropertyInfo ReasonTextInfo
		{
			get { return GetZPropertyInfo(nameof(ReasonText)); }
		}

		#endregion

		#region What changes do you recommend to content

		public ZString RecommendToContentText
		{
			get { return recommendToContentText; }
			set
			{
				recommendToContentText = value;
				RecommendToContentTextInfo.RefreshBinding();
			}
		}

		ZString recommendToContentText;

		public ZPropertyInfo RecommendToContentTextInfo
		{
			get { return GetZPropertyInfo(nameof(RecommendToContentText)); }
		}

		#endregion

		#region Hew can we make this content easier

		public ZString ContentEasierText
		{
			get { return contentEasierText; }
			set
			{
				contentEasierText = value;
				ContentEasierTextInfo.RefreshBinding();
			}
		}

		ZString contentEasierText;

		public ZPropertyInfo ContentEasierTextInfo
		{
			get { return GetZPropertyInfo(nameof(ContentEasierText)); }
		}

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateComment();
			ValidateReasonText();
			ValidateLinksToExistingContent();
			ValidateRecommendToContentTextAndContentEasierText();
			ValidateResolutionMethod();
		}

		protected override void ValidateComment()
		{
			CommentInfo.ClearAllNotifications();

			if (ResolutionMethod == SupportIncidentLookups.DispositionList.Constants.Closed.Other && Comment.Length < 20)
			{
				CommentInfo.AddError("The resolution comment must be atleast 20 characters when the Resolution Method is Other.");
			}
		}

		protected void ValidateReasonText()
		{
			ReasonTextInfo.ClearAllNotifications();

			if (OtherOption)
			{
				MandatoryValidation.CheckEntered(ReasonTextInfo);
			}
		}

		protected void ValidateLinksToExistingContent()
		{
			LinksToExistingContentInfo.ClearAllNotifications();

			if (PartlySolvedOption && (ContentBeDevelopYesOption || ContentBeDevelopNotWorthOption))
			{
				MandatoryValidation.CheckEntered(LinksToExistingContentInfo);
			}
		}

		protected void ValidateRecommendToContentTextAndContentEasierText()
		{
			RecommendToContentTextInfo.ClearAllNotifications();
			ContentEasierTextInfo.ClearAllNotifications();

			if ((PartlySolvedOption || ContentCouldNotBeFoundOption) && ContentBeDevelopYesOption)
			{
				MandatoryValidation.CheckEntered(RecommendToContentTextInfo);
				MandatoryValidation.CheckEntered(ContentEasierTextInfo);
			}
		}

		public void ValidateResolutionMethod()
		{
			ResolutionMethodInfo.ClearAllNotifications();
			if ((PartlySolvedOption || ContentCouldNotBeFoundOption) && ContentIsIrrelevantOption)
			{
				ListValidation.ErrorIfInvalidCode(ResolutionMethodInfo, GetCloseStatusDispositionList());
				MandatoryValidation.CheckEntered(ResolutionMethodInfo);
			}
		}

		public void ReadXml(XmlReader reader)
		{
			reader.MoveToContent();

			while (reader.MoveToNextAttribute())
			{
				var property = GetType().GetProperty(reader.Name,
				BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

				if (property != null && property.CanWrite && property.PropertyType != typeof(SupportIncident))
				{
					var propertyType = property.PropertyType;

					object value = null;

					// Check for custom type handling (e.g., ZString)
					if (propertyType == typeof(ZString) || propertyType == typeof(ZBool))
					{
						// Assuming ZString has a constructor that takes a string
						value = Activator.CreateInstance(propertyType, reader.Value);
					}
					else
					{
						// For normal types like string, int, DateTime, etc.
						value = Convert.ChangeType(reader.Value, propertyType);
					}

					property.SetValue(this, value);
				}
			}

			reader.Read(); // Move past this element
		}

		public void WriteXml(XmlWriter writer)
		{
			var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			foreach (var property in properties)
			{
				if (property.CanWrite && property.PropertyType != typeof(SupportIncident))
				{
					var value = property.GetValue(this);

					if (value != null)
					{
						string stringValue = null;

						// Check for custom type handling (e.g., ZString)
						if (value is ZString zString)
						{
							// Assuming ZString has a ToString() method
							stringValue = zString.ToString();
						}
						else
						{
							stringValue = value.ToString();
						}

						writer.WriteAttributeString(property.Name, stringValue);
					}
				}
			}
		}
		#endregion
	}
}
