using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question8To11ControlBag))]
	sealed class Question8To11ControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Question8To11ControlBag.Instance.Question8Label);
				yield return nameof(Question8To11ControlBag.Instance.Question8ALabel);
				yield return nameof(Question8To11ControlBag.Instance.Question8ADropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question8BLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question8BDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question8CLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question8CDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question8DLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question8DDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question9Label);
				yield return nameof(Question8To11ControlBag.Instance.Question9ALabel);
				yield return nameof(Question8To11ControlBag.Instance.Question9ADropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question9BLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question9BDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question10Label);
				yield return nameof(Question8To11ControlBag.Instance.Question10ALabel);
				yield return nameof(Question8To11ControlBag.Instance.Question10ADropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question10BLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question10BDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question10CLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question10CDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question10DLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question10DDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question11Label);
				yield return nameof(Question8To11ControlBag.Instance.Question11ALabel);
				yield return nameof(Question8To11ControlBag.Instance.Question11ADropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question11BLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question11BDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question11CLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question11CDropEdit);
				yield return nameof(Question8To11ControlBag.Instance.Question11DLabel);
				yield return nameof(Question8To11ControlBag.Instance.Question11DDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Question8To11ControlBag.Instance;
	}
}
