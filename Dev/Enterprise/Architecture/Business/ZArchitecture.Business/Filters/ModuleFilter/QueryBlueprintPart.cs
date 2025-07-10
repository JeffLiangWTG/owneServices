using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public class QueryBlueprintPart
	{
		public QueryBlueprintPart(ZQuery query, FilterOrCategory colour, JoinCondition join)
		{
			this.Query = query;
			this.Colour = colour;
			this.Join = join;
			Children = new List<QueryBlueprintPart>();
			OrPath = -1;
			orCount = -1;
		}

		public ZQuery Query;
		public FilterOrCategory Colour;
		public JoinCondition Join;
		public List<QueryBlueprintPart> Children;
		public int OrPath;
		public int OrCount
		{
			get
			{
				if (orCount == -1)
				{
					orCount = Children.Count(x => x.Join == JoinCondition.Or);
				}
				return orCount;
			}
		}
		int orCount;

		public void Add(QueryBlueprintPart part)
		{
			Children.Add(part);
		}

		public ZQuery Construct()
		{
			//logic: copy all children, unless we're specifying a specific OrPath right now - and in that case, copy only the Or matching the OrPath

			var result = Query.DeepClone(); //TODO: or shallow clone?
			var currentOrCount = 0;
			foreach (var child in Children)
			{
				var add = false;
				if (OrPath > -1 && child.Join == JoinCondition.Or)
				{
					if (currentOrCount == OrPath)
					{
						add = true;
					}
					++currentOrCount;
				}
				else
				{
					add = true;
				}
				if (add)
				{
					result.AddToFilter(child.Construct(), child.Join, true);
				}
			}
			return result;
		}

		public List<QueryBlueprintPart> FindOrJunctions(Dictionary<FilterOrCategory, bool> colourKey)
		{
			var result = new List<QueryBlueprintPart>();

			FindOrJunctionsCore(result, colourKey);

			return result;
		}

		void FindOrJunctionsCore(List<QueryBlueprintPart> result, Dictionary<FilterOrCategory, bool> colourKey)
		{
			if (OrCount > 1)
			{
				//if our colour is specified, then use that true/false; else, use None true/false
				//all children should have the same colour (this seems to be true in practice because we're already grouping by colour)
				var colour = Children.First(x => x.Join == JoinCondition.Or).Colour;

				if (colourKey.ContainsKey(colour))
				{
					if (colourKey[colour])
					{
						result.Add(this);
					}
				}
				else
				{
					if (colourKey[FilterOrCategory.None])
					{
						result.Add(this);
					}
				}
			}

			foreach (var child in Children)
			{
				child.FindOrJunctionsCore(result, colourKey);
			}
		}
	}
}
