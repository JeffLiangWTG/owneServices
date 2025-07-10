using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ResourceStrings.Business
{
	public class TopLevelTranslationFeedbackCollection : StmTranslationFeedbackCollection
	{
		public TopLevelTranslationFeedbackCollection(BusinessObjectFactory factory)
			: base(factory)
		{
			factory.Saved += new BusinessObjectFactory.SavedEventHandler(factory_Saved);
		}

		public override void Add(BusinessObject businessObject)
		{
			var feedback = (StmTranslationFeedback)businessObject;
			if (newItemsBySource.TryGetValue(feedback, out var items))
			{
				foreach (var existingItem in items)
				{
					if (existingItem.ResourceStringKey != feedback.ResourceStringKey)
					{
						using (existingItem.SuspendSettingHasChanges())
						{
							existingItem.PrimaryContext = null;
						}
					}
					return;
				}
			}
			else if (!feedback.IsInDatabase)
			{
				items = new List<StmTranslationFeedback>();
				newItemsBySource.Add(feedback, items);
			}
			if (!feedback.IsInDatabase)
			{
				items.Add(feedback);
			}

			base.Add(businessObject);
		}

		protected override void OnRemoving(BusinessObject bizO)
		{
			var feedback = (StmTranslationFeedback)bizO;
			if (newItemsBySource.TryGetValue(feedback, out var items))
			{
				items.Remove(feedback);
				if (items.Count == 0)
				{
					newItemsBySource.Remove(feedback);
				}
			}

			base.OnRemoving(bizO);
		}

		readonly Dictionary<StmTranslationFeedback, List<StmTranslationFeedback>> newItemsBySource = new Dictionary<StmTranslationFeedback, List<StmTranslationFeedback>>(new StmTranslationFeedbackMatchBySourceComparer());

		class StmTranslationFeedbackMatchBySourceComparer : IEqualityComparer<StmTranslationFeedback>
		{
			public bool Equals(StmTranslationFeedback x, StmTranslationFeedback y)
			{
				return x.XT_Source == y.XT_Source && x.XT_OriginalTranslation == y.XT_OriginalTranslation;
			}

			public int GetHashCode(StmTranslationFeedback obj)
			{
				return obj.XT_Source.GetHashCode() ^ obj.XT_OriginalTranslation.GetHashCode();
			}
		}

		void factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			newItemsBySource.Clear();

			foreach (var language in this.Cast<StmTranslationFeedback>().Select(f => f.XT_Language).Distinct())
			{
				ResourceStringCacheBuilder.Instance.ResetCache(language);
			}
			Res.NotifyChange();
			TranslationFeedbackFactory.UpdateCache(this);
		}

		public bool ContainsExactMatch()
		{
			bool containsExactMatch = false;
			foreach (StmTranslationFeedback entry in this)
			{
				if (entry.MatchType == TranslationFeedbackMatchTypes.Codes.Exact)
				{
					containsExactMatch = true;
					break;
				}
			}
			return containsExactMatch;
		}
	}
}
