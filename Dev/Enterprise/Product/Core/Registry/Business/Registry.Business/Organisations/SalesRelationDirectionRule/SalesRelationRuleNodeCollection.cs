using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Registry.Business
{
	public class SalesRelationRuleNodeCollection : NonPersistentBusinessObjectCollection<SalesRelationRuleNode>
	{
		public SalesRelationRuleNode FindSucceedingNode(ZString[] startingNodeSequence)
		{
			var actualSequenceIter = Elements.Cast<SalesRelationRuleNode>().GetEnumerator();
			var checkSequenceIter = startingNodeSequence.Cast<ZString>().GetEnumerator();

			while (checkSequenceIter.MoveNext())
			{
				if (!actualSequenceIter.MoveNext())
				{
					return null;
				}
				else if (actualSequenceIter.Current.Type == SalesRelationRuleNodeAdditionalTypesList.Codes.AnyNumberOfActivities)
				{
					return actualSequenceIter.Current;
				}
				else if (actualSequenceIter.Current.Type != SalesRelationRuleNodeAdditionalTypesList.Codes.AnySingleActivity && checkSequenceIter.Current != actualSequenceIter.Current.Type)
				{
					return null;
				}
			}

			return actualSequenceIter.MoveNext() ? actualSequenceIter.Current : null;
		}

		public bool IsLastNode(SalesRelationRuleNode node)
		{
			return Elements.IndexOf(node) == Elements.Count() - 1;
		}

		#region Implemention

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new SalesRelationRuleNode();
		}

		#endregion
	}
}
