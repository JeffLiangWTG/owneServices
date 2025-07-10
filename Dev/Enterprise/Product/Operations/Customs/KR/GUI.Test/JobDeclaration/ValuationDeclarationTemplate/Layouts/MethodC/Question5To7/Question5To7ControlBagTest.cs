using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(Question5To7ControlBag))]
	sealed class Question5To7ControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ALabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ALongLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ADropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5BLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5BLongLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5BDropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5CLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5CDropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5DLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5DDropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ELabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5EDropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ETextLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question5ETextBox);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question6ALabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question6ADropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question6BLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question6BDropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question7ALabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question7ADropEdit);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question7BLabel);
				yield return nameof(Question5To7ControlBag.InstanceFor5SM.Question7BDropEdit);
			}
		}

		protected override ControlBag GetControlBagForTesting() => Question5To7ControlBag.InstanceFor5SM;
	}
}
