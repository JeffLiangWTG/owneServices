using CargoWise.Types;
using Enterprise.Accounting.Registry.Business;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocContainerCollectionHelper
	{
		public DocContainerCollectionHelper(int maxContainersWithTypeOnLine, bool includeMode)
		{
			this.MaxContainersWithTypeOnLine = maxContainersWithTypeOnLine;
			this.IncludeMode = includeMode;
		}

		readonly int MaxContainersWithTypeOnLine;
		readonly bool IncludeMode;

		public ZString ContainerNumberAndType(IDocSimpleContainerCollection containers)
		{
			return ContainerNumberAndType(containers, false);
		}

		public ZString ContainerNumberAndType(IDocSimpleContainerCollection containers, bool includeClientRef)
		{
			return ContainerNumberAndType(containers, includeClientRef, true);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "Matching a constant string value")]
		public ZString ContainerNumberAndType(IDocSimpleContainerCollection containers, bool includeClientRef, bool separatePageForContainersPosible)
		{
			ZString result = "";
			int count = 0;
			foreach (IDocSimpleContainer container in containers)
			{
				if (container != null && container.ContainerNumber != "ALL CONTAINERS")
				{
					count++;
					if (count > MaxContainersWithTypeOnLine)
					{
						if (separatePageForContainersPosible &&
							AccountingConfigurationRegistry.Instance.ShowFullListingOfContainerNumbersOnSeparatePage.Value)
						{
							result = Res.GetString("b8af6f6a-abdf-4f6b-9d45-a0e5c1e90ce5", "See attached Container Details list.");
						}
						else
						{
							result = result.TrimEndIncludingWhiteSpace(',') + " ...";
						}
						break;
					}
					result += container.ContainerNumber;

					string details = "";
					if (IncludeMode)
					{
						details = container.Type;
					}

					details += (details.Length > 0 && container.Container != null ? "/" : "");
					details += (container.Container == null ? "" : container.Container.Code.ToString());

					if (includeClientRef)
					{
						details += container.ClientRef.IsEmpty ? ZString.Empty.ToString() : ", " + container.ClientRef.ToString();
					}

					if (details.Length > 0)
					{
						details = " (" + details + "), ";
					}
					else
					{
						details = ", ";
					}
					result += details;
				}
			}
			return result.TrimEndIncludingWhiteSpace(',');
		}
	}
}
