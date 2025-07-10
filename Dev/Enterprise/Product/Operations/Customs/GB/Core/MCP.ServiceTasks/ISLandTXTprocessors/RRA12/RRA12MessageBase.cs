using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.HoldAdderRemoverClearer;
using Enterprise.Customs.GB.Business.Interfaces;
using Enterprise.Customs.GB.MCP.ServiceTasks.RRA12;
namespace Enterprise.Customs.GB.MCP.ServiceTasks.RraProcessors.RRA12
{
	public abstract class RRA12MessageBase : IPortAuthorityHoldApplicationProvider
	{
		public RRA12MessageBase()
		{
			this.Lines = new List<RRA12MessageLine>();
			this.Header = new RRA12MessageHeader();
		}

		public string HoldType
		{
			get
			{
				PortHoldAuthorities auths = new PortHoldAuthorities();
				return auths.GetCodeFromDescription(HoldAuthority);
			}
		}

		public string HoldAuthority
		{
			get
			{
				List<string> knownAuthorities = new List<string>();
				foreach (RRA12MessageLine line in Lines)
				{
					foreach (string comment in line.Comments)
					{
						if (!knownAuthorities.Contains(comment))
						{
							knownAuthorities.Add(comment);
						}
					}
				}

				if (knownAuthorities.Count == 0)
				{
					return string.Empty; // no hold
				}
				else if (knownAuthorities.Count == 1)
				{
					return knownAuthorities[0];  // a specfic authority
				}
				else
				{
					return PortHoldAuthorities.Descriptions.MultipleAuthorities;
				}
			}
		}

		public AddOrRemove DirectionOfApplication
		{
			get
			{
				if (string.IsNullOrEmpty(HoldType))
				{
					return AddOrRemove.Cleared;
				}
				else
				{
					return AddOrRemove.HoldAdd;
				}
			}
		}

		public ZDateTime Date
		{
			get
			{
				return Header.AdviceDate;
			}
		}

		public RRA12MessageHeader Header { get; set; }
		public List<RRA12MessageLine> Lines { get; set; }
		public string PrettyText { get; set; }
		public abstract T CreateFromString<T>(string formattedRRA12Text) where T : RRA12MessageBase, new();
	}
}
