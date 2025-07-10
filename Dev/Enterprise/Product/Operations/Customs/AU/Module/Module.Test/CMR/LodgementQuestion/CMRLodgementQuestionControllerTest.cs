using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Module.Testing
{
	[TestedType(typeof(CMRLodgementQuestionController))]
	sealed class CMRLodgementQuestionControllerTest : CMRSearchOnlyControllerTest
	{
		protected override ControllerID GetControllerID() => ControllerIDs.Customs.AU.CMRLodgementQuestion;

		protected override Type GetBusinessObjectType() => typeof(CMRLodgementQuestion);
	}
}
