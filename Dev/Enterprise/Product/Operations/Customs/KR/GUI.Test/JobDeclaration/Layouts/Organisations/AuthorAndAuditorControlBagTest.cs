using System.Collections.Generic;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.KR.GUI.Testing
{
	[TestedType(typeof(AuthorAndAuditorControlBag))]
	sealed class AuthorAndAuditorControlBagTest : ControlBagAbstractTest
	{
		protected override IEnumerable<string> RegisteredControlNames
		{
			get
			{
				yield return nameof(AuthorAndAuditorControlBag.AuthorGuidDropEdit);
				yield return nameof(AuthorAndAuditorControlBag.AuthorNameTextBox);
				yield return nameof(AuthorAndAuditorControlBag.AuthorPhoneTextBox);
				yield return nameof(AuthorAndAuditorControlBag.AuthorJobTitleTextBox);
				yield return nameof(AuthorAndAuditorControlBag.AuditorGuidDropEdit);
				yield return nameof(AuthorAndAuditorControlBag.AuditorNameTextBox);
				yield return nameof(AuthorAndAuditorControlBag.AuditorPhoneTextBox);
				yield return nameof(AuthorAndAuditorControlBag.AuditorJobTitleTextBox);
			}
		}

		protected override ControlBag GetControlBagForTesting() => AuthorAndAuditorControlBag.Instance;
	}
}
