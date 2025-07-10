using System.Collections.Generic;
using System.Linq;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(ValidationRuleMessages))]
sealed class ValidationRuleMessagesTest : ValidationRuleMessagesAbstractTest<ValidationRuleMessages>
{
	protected override Dictionary<string, string> RuleCodeReplacements => new Dictionary<string, string>()
	{
		{ "C0101-1", "C0101, R0859" },
		{ "R0364-2", "NP70020" },
		{ "TR0022", "NP70025" },
		{ "NR0020", "NP70026" },
		{ "R0103", "NP70042" },
		{ "R0315", "NP70044" },
		{ "TR0030-1", "NP70057" },
		{ "R0020-1", "NP70059" },
		{ "R0507-1", "NP70067" },
		{ "E1109", "NP70071" },
		{ "E1109-1", "NP70071" },
		{ "R0318", "NP70077" },
		{ "NR0026", "NP70079" },
		{ "NR0028", "NP70116" },
		{ "R0223", "NP70132" },
		{ "NR0027", "NP70178" },
		{ "TR0019", "NP70210" },
		{ "R0023","NP70211" },
		{ "TR0063", "NS30005" },
		{ "TR0065", "NS30006" },
		{ "B1858-2", "NS30011" },
		{ "NR0036", "NS30014" },
		{ "C0001-2", "NS30018" },
		{ "C0045", "NS30019" },
		{ "C0060-1", "NS30021" },
		{ "C0060-2", "NS30021" },
		{ "C0060-3", "NS30021" },
		{ "C0085-1", "NS30022" },
		{ "C0086-1", "NS30023" },
		{ "C0191-1", "NS30024" },
		{ "B1848", "NS30026" },
		{ "C0586", "NS30026" },
		{ "C0531", "NS30028" },
		{ "C0670", "NS30029" },
		{ "C0337-2", "NS30039" },
		{ "C0343-2", "NS30040" },
		{ "NR0037", "NS30041" },
		{ "C0035-1", "NS30045" },
		{ "C0502", "NS30046" },
		{ "C0599-2", "NS30064" },
		{ "TR0062", "NS30065" },
		{ "C0599-1", "NS30091" },
		{ "NR0031", "NS30106" },
		{ "TR0050", "NS30106" },
		{ "NR0039", "NS30112" },
		{ "C0015", "NS30119" },
		{ "NR0029", "NS30124" },
		{ "C0001-3", "NS30132" },
		{ "C0153-1", "NS30164" },
	};

	public void TestOrderedBySwissCode()
	{
		var replacements = RuleCodeReplacements;
		var sortedReplacements = replacements.OrderBy(x => x.Value).ThenBy(x => x.Key);
		AssertSequencesEqual(sortedReplacements, replacements);
	}
}
