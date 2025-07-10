using System.Collections.Generic;
using CargoWise.Types;
using Res = DocumentWrappers.Res;

/// a list of containers may be supplied either as a list of individual containers, each with their unique container number supplied (in which case, count = 1 for each line)
/// or as a "container line" that gives no unique number, but does give the container type and a count of the number of containers of that type (>1). 

namespace Enterprise.DocumentWrappers
{
	public class GoodsDescriptionContainerListingHelper
	{
		#region Private Implementation stuff

		class ContainerGroup
		{
			public ContainerGroup(ZString contType, ZInt contCount)
			{
				this.ContainerType = contType;
				this.Count = contCount;
			}

			public ZString ContainerType { get; private set; }
			public ZInt Count { get; set; }
		}

		int FindContainerGroupIndex(ZString containerType)
		{
			int result = 0;

			foreach (ContainerGroup tmp in listOfContainerGroups)
			{
				if (tmp.ContainerType == containerType)
				{
					return result;
				}
				result++;
			}

			return NotFound;
		}

		const int NotFound = -1;
		readonly List<ContainerGroup> listOfContainerGroups = new List<ContainerGroup>();
		readonly List<ZString> listOfUniqueContainers = new List<ZString>();

		#endregion

		#region Public Interface

		public void AddContainer(ZString containerNumber, ZString containerType, ZInt containerCount)
		{
			bool isAContainerGroup = containerNumber.Length == 0;
			bool containerNotAlreadyProcessed = !listOfUniqueContainers.Contains(containerNumber);

			if (isAContainerGroup || containerNotAlreadyProcessed)
			{
				TotalCountOfContainers += containerCount;
				listOfUniqueContainers.Add(containerNumber);

				int i = FindContainerGroupIndex(containerType);
				if (i == NotFound)
				{
					listOfContainerGroups.Add(new ContainerGroup(containerType, containerCount));
				}
				else
				{
					listOfContainerGroups[i].Count += containerCount;
				}
			}
		}

		public ZString ContainersListForDocument
		{
			get
			{
				ZString result = ZString.Empty;

				foreach (ContainerGroup group in listOfContainerGroups)
				{
					result += Res.GetString("c68a7ed1-b933-46fd-9544-a34f65b8cc7e", "{0} x {1} CONTAINER", group.Count.ToString(), group.ContainerType) + "\n";
				}

				return result;
			}
		}

		public List<ZString> ContainerTypeList
		{
			get
			{
				List<ZString> result = new List<ZString>();

				foreach (ContainerGroup group in listOfContainerGroups)
				{
					result.Add(Res.GetString("44cddbf8-8607-496d-8466-5ac72ae4fba8", "x {0} CONTAINER", group.ContainerType));
				}
				return result;
			}
		}

		public List<ZString> ContainerCountList
		{
			get
			{
				List<ZString> result = new List<ZString>();

				foreach (ContainerGroup group in listOfContainerGroups)
				{
					result.Add(group.Count.ToString());
				}
				return result;
			}
		}

		public int TotalCountOfContainers
		{
			get;
			private set;
		}

		#endregion
	}
}
