using System.Collections.Generic;
using System.Linq;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;
using Glow = CargoWise.Definitions;

namespace Enterprise.ZArchitecture.GlowCompatibility.Test
{
	class EventCodesTest : TestCase
	{
		public void TestAllGlowEventsAreInCW1()
		{
			var cw1Events = GetCW1Events();
			var glowEvents = Glow.EventDefinitions.All;

			var glowEventsNotInCW1 = glowEvents.Except(cw1Events);

			Assert(string.Format("The following GLOW events are not in CW1:\n\n{0}\n", string.Join("\n", glowEventsNotInCW1.Select(x => x.Code))), !glowEventsNotInCW1.Any());
		}

		public void TestAllCW1EventsAreInGlow()
		{
			var cw1Events = GetCW1Events();
			var glowEvents = Glow.EventDefinitions.All;

			var cw1EventsNotInGlow = cw1Events.Except(glowEvents);

			Assert(string.Format("The following CW1 events are not in GLOW:\n\n{0}\n", string.Join("\n", cw1EventsNotInGlow.Select(x => x.Code))), !cw1EventsNotInGlow.Any());
		}

		IEnumerable<Glow.EventDefinition> GetCW1Events() => AutoEvents.All.OfType<Event>().Select(x => new Glow.EventDefinition(x.PK.ToGuid(), x.Code, x.MultilingualDescription.GetUnresolvedString()));
	}
}
