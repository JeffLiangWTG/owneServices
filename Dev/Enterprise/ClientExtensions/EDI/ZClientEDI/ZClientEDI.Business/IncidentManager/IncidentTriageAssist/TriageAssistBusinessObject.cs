using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Shared;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using WTG.RtfConverter;
using WTG.RtfConverter.Dom;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class TriageAssistBusinessObject : AutoTriageAssistBusinessObject
	{
		public TriageAssistBusinessObject(ITriageAssistParent parent) : base(parent.Factory)
		{
			Parent = parent;
			RegisterEditableChildObject(Parent);
			Product = Parent.Product;
			Parent.TriagePKInfo.ValueChanged += TriagePKInfo_ValueChanged;
			SearchTermOperator = ComparisonConstants.ContainsAny;
			CriteriaTypeFilter = ComparisonConstants.Any;
			ShouldSearchKeywords = true;
			ShouldSearchDescription = true;
		}

		public ITriageAssistParent Parent { get; }

		[List("ProductList")]
		[ResourceStringData("TriageAssistBusinessObject|Product", Caption = "Product")]
		public override ZString Product
		{
			get => base.Product;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.Product = value;
				}
			}
		}

		#region SearchTerm

		public override ZString SearchTerm
		{
			get => base.SearchTerm;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.SearchTerm = value;
					SearchTermEndsWithSpace = value.EndsWith(" ");

					var pattern = @"""[^""]+""|\S+";
					var matches = Regex.Matches(SearchTerm, pattern);
					var tokens = new HashSet<string>();

					foreach (Match match in matches)
					{
						var token = match.Value;
						if (token.StartsWith("\"") && token.EndsWith("\""))
						{
							token = token.Trim('"', '"');
						}
						if (!string.IsNullOrWhiteSpace(token))
						{
							tokens.Add(token);
						}
					}
					SearchTermTokens = tokens;
				}
			}
		}

		public bool SearchTermEndsWithSpace { get; private set; }
		public IEnumerable<string> SearchTermTokens { get; private set; } = Enumerable.Empty<string>();

		[List("SearchTermOperatorList")]
		public override ZString SearchTermOperator
		{
			get => base.SearchTermOperator;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.SearchTermOperator = value;
					RefreshSearchTerm();
				}
			}
		}

		public CodeDescriptionPairList SearchTermOperatorList
		{
			get
			{
				return Factory.GetCachedValue("TriageAssistBusinessObject.SearchTermOperatorList",
					() =>
					{
						var list = new CodeDescriptionPairList();
						list.AddPair((NoResString)ComparisonConstants.Exact);
						list.AddPair((NoResString)ComparisonConstants.ContainsAny);
						list.AddPair((NoResString)ComparisonConstants.ContainsAll);
						return list;
					});
			}
		}

		[List("CriteriaTypeFilterList")]
		public override ZString CriteriaTypeFilter
		{
			get => base.CriteriaTypeFilter;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.CriteriaTypeFilter = value;
					RefreshSearchTerm();
				}
			}
		}

		public ZString CriteriaTypeFilterCode => IncidentDiagnosticCriteriaTypeList.GetCodeFromDescription(CriteriaTypeFilter);

		public CodeDescriptionPairList CriteriaTypeFilterList
		{
			get
			{
				return Factory.GetCachedValue("TriageAssistBusinessObject.CriteriaTypeFilterList",
					() =>
					{
						var result = new CodeDescriptionPairList();
						result.AddPair((NoResString)ComparisonConstants.Any);
						foreach(ICodeDescription item in IncidentDiagnosticCriteriaTypeList)
						{
							result.AddPair(item.Description);
						}
						return result;
					});
			}
		}

		CodeDescriptionPairList IncidentDiagnosticCriteriaTypeList
			=> Factory.GetCachedValue("TriageAssistBusinessObject.IncidentDiagnosticCriteriaTypeList", () => new IncidentDiagnosticCriteriaTypes());

		public override ZBool ShowSearchOptions
		{
			get => base.ShowSearchOptions;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.ShowSearchOptions = value;
				}
			}
		}

		public override ZBool ShouldSearchKeywords
		{
			get => base.ShouldSearchKeywords;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.ShouldSearchKeywords = value;
					RefreshSearchTerm();
				}
			}
		}

		public override ZBool ShouldSearchDescription
		{
			get => base.ShouldSearchDescription;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.ShouldSearchDescription = value;
					RefreshSearchTerm();
				}
			}
		}

		public override ZBool ShouldSearchSuggestedList
		{
			get => base.ShouldSearchSuggestedList;
			set
			{
				using (SuspendSettingHasChanges())
				{
					base.ShouldSearchSuggestedList = value;
					FilteredSuggestedCriteriaCollection.Rebuild();
				}
			}
		}

		void RefreshSearchTerm()
		{
			if (!SearchTerm.IsEmpty)
			{
				SearchedCriteriaCollection.LoadBySearchTerm();
			}

			if (ShouldSearchSuggestedList)
			{
				FilteredSuggestedCriteriaCollection.Rebuild();
			}
		}

		public void RefreshSearchTerm(string searchTerm, string product)
		{
			SearchTerm = searchTerm;
			Product = product;
			SearchedCriteriaCollection.LoadBySearchTerm();
			if (ShouldSearchSuggestedList)
			{
				FilteredSuggestedCriteriaCollection.Rebuild();
			}
		}

		#endregion

		#region Focused Objects

		public override ZBool ShowFocusedObjectsOnly
		{
			get => base.ShowFocusedObjectsOnly;
			set
			{
				base.ShowFocusedObjectsOnly = value;
				UpdateSubShowFocusedFlags();
			}
		}

		public override ZBool ShowFocusedSuggestedCriteriaOnly
		{
			get => base.ShowFocusedSuggestedCriteriaOnly;
			set
			{
				base.ShowFocusedSuggestedCriteriaOnly = value;
				UpdateMainShowFocusedFlag();
				FilteredSuggestedCriteriaCollection.Rebuild();
			}
		}

		public override ZBool ShowFocusedTriageNodesOnly
		{
			get => base.ShowFocusedTriageNodesOnly;
			set
			{
				base.ShowFocusedTriageNodesOnly = value;
				UpdateMainShowFocusedFlag();
				FilteredTriageAssistTreeWrapperCollection.Rebuild();
			}
		}

		bool IsUpdatingFocusedObjects;

		void UpdateMainShowFocusedFlag()
		{
			if (!IsUpdatingFocusedObjects)
			{
				using (new DisposableAction(() =>
				{
					IsUpdatingFocusedObjects = true;
					ShowFocusedObjectsOnly = ShowFocusedSuggestedCriteriaOnly || ShowFocusedTriageNodesOnly;
					OnShowFocusedObjectsOnlyChanged?.Invoke(this, EventArgs.Empty);
				}, () => IsUpdatingFocusedObjects = false))
				{
				}
			}
		}

		public event EventHandler OnShowFocusedObjectsOnlyChanged;

		void UpdateSubShowFocusedFlags()
		{
			if (!IsUpdatingFocusedObjects)
			{
				using (new DisposableAction(() =>
				{
					IsUpdatingFocusedObjects = true;
					ShowFocusedSuggestedCriteriaOnly = ShowFocusedTriageNodesOnly = ShowFocusedObjectsOnly;
				}, () => IsUpdatingFocusedObjects = false))
				{
				}
			}
		}

		#endregion

		#region Collections

		[ChildEditable]
		public RelevantDiagnosticCriteriaCollection SearchedCriteriaCollection
		{
			get
			{
				if (searchedCriteriaCollection == null)
				{
					searchedCriteriaCollection = new RelevantDiagnosticCriteriaCollection(this);
					RegisterEditableChildObject(searchedCriteriaCollection);
				}

				return searchedCriteriaCollection;
			}
		}

		RelevantDiagnosticCriteriaCollection searchedCriteriaCollection;

		[ChildEditable]
		public RelevantDiagnosticCriteriaCollection LinkedCriteriaCollection
		{
			get
			{
				if (linkedCriteriaCollection == null)
				{
					linkedCriteriaCollection = new RelevantDiagnosticCriteriaCollection(this);
					RegisterEditableChildObject(linkedCriteriaCollection);
				}

				return linkedCriteriaCollection;
			}
		}

		RelevantDiagnosticCriteriaCollection linkedCriteriaCollection;

		[ChildEditable]
		public RelevantDiagnosticCriteriaCollection SuggestedCriteriaCollection
		{
			get
			{
				if (suggestedCriteriaCollection == null)
				{
					suggestedCriteriaCollection = new RelevantDiagnosticCriteriaCollection(this);
					RegisterEditableChildObject(suggestedCriteriaCollection);
				}

				return suggestedCriteriaCollection;
			}
		}

		RelevantDiagnosticCriteriaCollection suggestedCriteriaCollection;

		public FilteredRelevantDiagnosticCriteriaCollection FilteredSuggestedCriteriaCollection
		{
			get
			{
				if (filteredSuggestedCriteriaCollection == null)
				{
					filteredSuggestedCriteriaCollection = new FilteredSuggestedRelevantDiagnosticCriteriaCollection(this);
				}
				return filteredSuggestedCriteriaCollection;
			}
		}

		FilteredRelevantDiagnosticCriteriaCollection filteredSuggestedCriteriaCollection;

		public TriageAssistTreeBizObjWrapperCollection TriageAssistTreeWrapperCollection
		{
			get
			{
				if (triageAssistTreeWrapperCollection == null)
				{
					triageAssistTreeWrapperCollection = new TriageAssistTreeBizObjWrapperCollection(this);
				}

				return triageAssistTreeWrapperCollection;
			}
		}

		TriageAssistTreeBizObjWrapperCollection triageAssistTreeWrapperCollection;

		public FilteredTriageAssistTreeBizObjWrapperCollection FilteredTriageAssistTreeWrapperCollection
		{
			get
			{
				if (filteredTriageAssistTreeWrapperCollection == null)
				{
					filteredTriageAssistTreeWrapperCollection = new FilteredTriageAssistTreeBizObjWrapperCollection(this);
				}
				return filteredTriageAssistTreeWrapperCollection;
			}
		}

		FilteredTriageAssistTreeBizObjWrapperCollection filteredTriageAssistTreeWrapperCollection;

		public CodeDescriptionPairList ProductList
		{
			get
			{
				return Factory.GetCachedValue("TriageAssistBusinessObject.ProductList",
					() =>
					{
						return IncidentDetailsLookupsHelper.ProductList;
					});
			}
		}

		void OnPropertyValueChanged(object sender, ZPropertyValueChangedEventArgs e)
		{
			if (OnPropertyValueChangedSuspender.IsSuspended)
			{
				return;
			}

			if (sender is RelevantDiagnosticCriteria relevantCriteria)
			{
				if (relevantCriteria.ParentCollections.Any())
				{
					var linkedCriteriaCollectionChanged = false;
					if (new[] { nameof(RelevantDiagnosticCriteria.Confirm),
								nameof(RelevantDiagnosticCriteria.Negate),
								nameof(RelevantDiagnosticCriteria.Investigate) }.Contains(e.Property.Name))
					{
						if (e.Property.Value.Equals(ZBool.True))
						{
							LinkedCriteriaCollection.Add(relevantCriteria);
							SuggestedCriteriaCollection.Remove(relevantCriteria);
							linkedCriteriaCollectionChanged = true;
						}
						else
						{
							if (!relevantCriteria.Confirm && !relevantCriteria.Negate && !relevantCriteria.Investigate)
							{
								LinkedCriteriaCollection.Remove(relevantCriteria);
								linkedCriteriaCollectionChanged = true;
							}
						}
					}

					if (linkedCriteriaCollectionChanged)
					{
						RefreshTriageNodes();
					}
				}
			}
		}

		public ActionSuspender OnPropertyValueChangedSuspender { get; } = new ActionSuspender();

		protected override void OnFactorySaved(bool saveSucceeded)
		{
			base.OnFactorySaved(saveSucceeded);
			if (saveSucceeded)
			{
				LinkedCriteriaCollection.SortByActionOption();
				OnTriageAssistSaved?.Invoke(this, EventArgs.Empty);
			}
		}

		protected override void OnFactorySaving()
		{
			base.OnFactorySaving();
			if (!Parent.TriagePK.IsEmpty && Parent.TriagePKInfo.HasChanges)
			{
				var logParamList = new List<KeyValuePair<string, string>>
				{
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.Description, $"Triage node applied to {Parent.TableName}"),
					new KeyValuePair<string, string>(CargoWise.EventReference.Constants.EventReferenceParameters.Codes.ReferenceNumber, Parent.IncidentTriage.IMT_TriageNumber)
				};
				Parent.Logs.AddNew(AutoEvents.MiscellaneousEvent, logParamList.ToArray());
			}
		}

		void RefreshTriageNodes()
		{
			TriageAssistTreeWrapperCollection.Load();
			SuggestedCriteriaCollection.LoadSuggestedCriteria();
			FilteredTriageAssistTreeWrapperCollection.Rebuild();
			FilteredSuggestedCriteriaCollection.Rebuild();
			RefreshTextProperties();
		}

		void TriagePKInfo_ValueChanged(object sender, EventArgs e) => RefreshTriageNodes();

		public event EventHandler OnTriageAssistSaved;

		#endregion

		#region Loaders

		public IEnumerable<RelevantDiagnosticCriteria> LoadRelevantCriteria(ZQuery incidentDiagnosticCriteriaQuery)
		{
			var criteria = Factory.Load<IncidentDiagnosticCriteria>(incidentDiagnosticCriteriaQuery);
			return criteria.Select(x =>
			{
				var relevantCriteria = Factory.Load<RelevantDiagnosticCriteria>(x.PK);
				if (relevantCriteria == null)
				{
					relevantCriteria = new RelevantDiagnosticCriteria(x, this);
					relevantCriteria.HasChanges = false;
					relevantCriteria.PropertyValueChanged += OnPropertyValueChanged;
				}
				return relevantCriteria;
			});
		}

		#endregion

		#region Validatons

		public override void ValidateProduct()
		{
			base.ValidateProduct();
			ProductInfo.ClearAllNotifications();
			if (Product != Parent.Product)
			{
				ProductInfo.AddWarning("This product does not match the incident.");
			}
		}

		public override void ValidateSearchTermOperator()
		{
			base.ValidateSearchTermOperator();
			MandatoryValidation.CheckEntered(SearchTermOperatorInfo);
			ListValidation.ErrorIfInvalidCode(SearchTermOperatorInfo);
		}

		public override void ValidateCriteriaTypeFilter()
		{
			base.ValidateCriteriaTypeFilter();
			MandatoryValidation.CheckEntered(CriteriaTypeFilterInfo);
			ListValidation.ErrorIfInvalidCode(CriteriaTypeFilterInfo);
		}

		public override void ValidateShouldSearchDescription()
		{
			ShouldSearchKeywordsInfo.ClearAllNotifications();
			base.ValidateShouldSearchDescription();
			if (!ShouldSearchKeywords && !ShouldSearchDescription)
			{
				ShouldSearchDescriptionInfo.AddError("Please select at least one option.");
			}
		}

		public override void ValidateShouldSearchKeywords()
		{
			ShouldSearchDescriptionInfo.ClearAllNotifications();
			base.ValidateShouldSearchKeywords();
			if (!ShouldSearchKeywords && !ShouldSearchDescription)
			{
				ShouldSearchKeywordsInfo.AddError("Please select at least one option.");
			}
		}

		#endregion

		#region Text Properties

		public void RefreshTextProperties()
		{
			DiagnosticGuideInternalSupportNoteRTFInfo.RefreshBinding();
			DiagnosticGuideClientQuestionRTFInfo.RefreshBinding();
			FinalisedTriageNodeInternalSupportActionRTFInfo.RefreshBinding();
			FinalisedTriageNodeClientMessageRTFInfo.RefreshBinding();
		}

		public ZInt DiagnosticGuideItemCount => LinkedCriteriaCollection.OfType<RelevantDiagnosticCriteria>()
										.Count(x => (x.Investigate || x.Confirm) && (!x.DiagnosticCriteria.IMD_InternalSupportNote.IsEmpty || !x.DiagnosticCriteria.IMD_Question.IsEmpty));
		ZString DiagnosticGuideInternalSupportNote
		{
			get
			{
				var investigateText = string.Join(System.Environment.NewLine, LinkedCriteriaCollection.Where(x => x.Investigate && !x.DiagnosticCriteria.IMD_InternalSupportNote.IsEmpty)
					.Select(x => $"*** {x.DiagnosticCriteria.IMD_Description} ***{System.Environment.NewLine}{x.DiagnosticCriteria.IMD_InternalSupportNote}{System.Environment.NewLine}"));
				var confirmText = string.Join(System.Environment.NewLine, LinkedCriteriaCollection.Where(x => x.Confirm && !x.DiagnosticCriteria.IMD_InternalSupportNote.IsEmpty)
					.Select(x => $"*** {x.DiagnosticCriteria.IMD_Description} ***{System.Environment.NewLine}{x.DiagnosticCriteria.IMD_InternalSupportNote}{System.Environment.NewLine}"));

				return GenerateGuideText(investigateText, confirmText);
			}
		}

		ZString DiagnosticGuideClientQuestion
		{
			get
			{
				var investigateText = string.Join(System.Environment.NewLine, LinkedCriteriaCollection.Where(x => x.Investigate && !x.DiagnosticCriteria.IMD_Question.IsEmpty)
					.Select(x => $"*** {x.DiagnosticCriteria.IMD_Description} ***{System.Environment.NewLine}{x.DiagnosticCriteria.IMD_Question}{System.Environment.NewLine}"));
				var confirmText = string.Join(System.Environment.NewLine, LinkedCriteriaCollection.Where(x => x.Confirm && !x.DiagnosticCriteria.IMD_Question.IsEmpty)
					.Select(x => $"*** {x.DiagnosticCriteria.IMD_Description} ***{System.Environment.NewLine}{x.DiagnosticCriteria.IMD_Question}{System.Environment.NewLine}"));

				return GenerateGuideText(investigateText, confirmText);
			}
		}

		public ZInt FinalisedTriageNodeActionCount => Parent.IncidentTriage?.ChecklistPivots.OfType<IncidentTriageChecklistItemPivot>()
					.Count(x => !x.ChecklistItem.PublishedDescriptionText.IsEmpty) ?? 0;

		ZString FinalisedTriageNodeInternalSupportAction =>
				string.Join(System.Environment.NewLine, Parent.IncidentTriage?.ChecklistPivots.Select(x => x.ChecklistItem)
					.Where(x => !x.IMC_IsPublished && !x.PublishedDescriptionText.IsEmpty)
					.Select(x => $"*** {x.IMC_SupportDescription} ***{System.Environment.NewLine}{x.PublishedDescriptionText}{System.Environment.NewLine}") ?? Enumerable.Empty<string>());

		ZString FinalisedTriageNodeClientMessage =>
				string.Join(System.Environment.NewLine, Parent.IncidentTriage?.ChecklistPivots.Select(x => x.ChecklistItem)
					.Where(x => x.IMC_IsPublished && !x.PublishedDescriptionText.IsEmpty)
					.Select(x => $"*** {x.IMC_SupportDescription} ***{System.Environment.NewLine}{x.PublishedDescriptionText}{System.Environment.NewLine}") ?? Enumerable.Empty<string>());

		#region RTF

		public ZBlob DiagnosticGuideClientQuestionRTF_HTML => ORtfTextUtil.RtfToHtml(DiagnosticGuideClientQuestionRTF);
		public ZBlob DiagnosticGuideClientQuestionRTF => ConvertTextToRtfBytes(DiagnosticGuideClientQuestion);
		public ZPropertyInfo DiagnosticGuideClientQuestionRTFInfo => GetZPropertyInfo(nameof(DiagnosticGuideClientQuestionRTF));

		public ZBlob DiagnosticGuideInternalSupportNoteRTF_HTML => ORtfTextUtil.RtfToHtml(DiagnosticGuideInternalSupportNoteRTF);
		public ZBlob DiagnosticGuideInternalSupportNoteRTF => ConvertTextToRtfBytes(DiagnosticGuideInternalSupportNote, true);
		public ZPropertyInfo DiagnosticGuideInternalSupportNoteRTFInfo => GetZPropertyInfo(nameof(DiagnosticGuideInternalSupportNoteRTF));

		public ZBlob FinalisedTriageNodeClientMessageRTF_HTML => ORtfTextUtil.RtfToHtml(FinalisedTriageNodeClientMessageRTF);
		public ZBlob FinalisedTriageNodeClientMessageRTF => ConvertTextToRtfBytes(FinalisedTriageNodeClientMessage);
		public ZPropertyInfo FinalisedTriageNodeClientMessageRTFInfo => GetZPropertyInfo(nameof(FinalisedTriageNodeClientMessageRTF));

		public ZBlob FinalisedTriageNodeInternalSupportActionRTF_HTML => ORtfTextUtil.RtfToHtml(FinalisedTriageNodeInternalSupportActionRTF);
		public ZBlob FinalisedTriageNodeInternalSupportActionRTF => ConvertTextToRtfBytes(FinalisedTriageNodeInternalSupportAction, true);
		public ZPropertyInfo FinalisedTriageNodeInternalSupportActionRTFInfo => GetZPropertyInfo(nameof(FinalisedTriageNodeInternalSupportActionRTF));

		static ZString GenerateGuideText(string investigateText, string confirmText)
		{
			var result = new ZStringBuilder();

			if (!string.IsNullOrWhiteSpace(investigateText))
			{
				result.AppendLine(RtfTagInvestigate);
				result.AppendLine();
				result.AppendLine(investigateText);
			}

			if (!string.IsNullOrWhiteSpace(confirmText))
			{
				result.AppendLine(RtfTagConfirm);
				result.AppendLine();
				result.AppendLine(confirmText);
			}

			return result.ToString();
		}

		static ZBlob ConvertTextToRtfBytes(string text, bool shouldShortenUrl = false)
		{
			var domTree = PlaintextParser.Parse(text)
				.Decode()
				.DetectLinks();

			domTree = domTree
				.Reduce<IWtgNode, IWtgNode>(c => new TextCondenser<IWtgNode>(c))
				.Reduce<IWtgNode, IWtgNode>(
				c =>
				{
					var b = new WtgDomTreeBuilder(c);
					return b.AsSafeVisitor() with
					{
						OnText = t =>
						{
							if (Equals(t, RtfTagInvestigate))
							{
								b.Open(new Phrase(Underline: true, Bold: true)).Add("INVESTIGATE");
							}
							else if (Equals(t, RtfTagConfirm))
							{
								b.Open(new Phrase(Underline: true, Bold: true)).Add("CONFIRM");
							}
							else
							{
								b.Add(shouldShortenUrl && b.OpenNodes.First() is Phrase { Link.Target: var target } && Equals(target, t) ? "Link" : t);
							}
						},
					};
				}
				);

			var rtf = domTree
				.Reduce<IWtgNode, IRtfNode>(c => new RtfEncoder(c, false))
				.Markup();

			return Encoding.UTF8.GetBytes(rtf);
		}

		const string RtfTagInvestigate = "<RTF_8b64f6689df4_INVESTIGATE>";
		const string RtfTagConfirm = "<RTF_a98642ed2b08_CONFIRM> ";

		#endregion

		#endregion

		public static class ComparisonConstants
		{
			public const string Exact = "Exact";
			public const string ContainsAny = "Contains Any";
			public const string ContainsAll = "Contains All";
			public const string Any = "Any";
		}
	}
}
