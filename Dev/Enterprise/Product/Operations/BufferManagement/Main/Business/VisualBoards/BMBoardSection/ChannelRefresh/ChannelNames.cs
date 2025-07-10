using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.BufferManagement.Business
{
	[Immutable]
	public class ChannelNames
	{
		internal ChannelNames(IEnumerable<ChannelNamePair> namesByCommunicability)
		{
			NamesByCommunicability = namesByCommunicability.ToImmutableArray();
		}

		public ImmutableArray<ChannelNamePair> NamesByCommunicability { get; }

		public string GetSpecificName(DisplayNameType requestedNameType)
		{
			return GetChannelNamePairOfGivenNameType(requestedNameType).Name;
		}

		public string GetDisplayableName(ZString channelEntityType) => GetDisplayableName(channelEntityType, c => !c.Name.IsEmpty);

		public string GetDisplayableName(ZString channelEntityType, Func<ChannelNamePair, bool> isSuitable)
		{
			if (BMSRegistry.Instance.ChannelHeadingsUsePreferredName.Value && channelEntityType == ChannelTypeList.Codes.Resource)
			{
				var nameTypePair = GetChannelNamePairOfGivenNameType(DisplayNameType.FriendlyName);
				if (isSuitable(nameTypePair))
				{
					return nameTypePair.Name;
				}
			}

			return NamesByCommunicability.Where(n => isSuitable(n)).First().Name;
		}

		ChannelNamePair GetChannelNamePairOfGivenNameType(DisplayNameType nameToGet)
		{
			return NamesByCommunicability.Where(pair => pair.DisplayNameType == nameToGet).First();
		}
	}

	[Immutable]
	public class ChannelNamePair
	{
		public ChannelNamePair(DisplayNameType type, ZString name)
		{
			DisplayNameType = type;
			Name = name;
		}

		public DisplayNameType DisplayNameType { get; }
		public ZString Name { get; }
	}

	public class ChannelNameBuilder
	{
		public ChannelNameBuilder(ZString fullName, ZString preferredName, ZString channelCode)
		{
			Add(DisplayNameType.FullName, fullName);
			Add(DisplayNameType.FriendlyName, preferredName);
			Add(DisplayNameType.ChannelCode, channelCode);
		}

		public ChannelNameBuilder(ZString fullName, ZString channelCode)
			: this(fullName, ZString.Empty, channelCode)
		{
		}

		SortedList<DisplayNameType, ChannelNamePair> SortedList { get; } = new SortedList<DisplayNameType, ChannelNamePair>();

		public ChannelNameBuilder Add(DisplayNameType type, ZString name)
		{
			SortedList.Add(type, new ChannelNamePair(type, name));
			return this;
		}

		public ChannelNames Build()
		{
			return new ChannelNames(SortedList.Values);
		}
	}
}
