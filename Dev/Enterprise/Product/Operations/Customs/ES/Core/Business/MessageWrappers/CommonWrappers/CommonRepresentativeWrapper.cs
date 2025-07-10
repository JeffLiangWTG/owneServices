using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.MessageWrappers;

public class CommonRepresentativeWrapper : PartyIdWrapper, ICommonRepresentative
{
	public static CommonRepresentativeWrapper New(JobDeclaration jobDeclaration)
	{
		var orgAddress = GetRepresentative(jobDeclaration);
		return orgAddress == null ? null : new CommonRepresentativeWrapper(orgAddress);
	}

	protected CommonRepresentativeWrapper(OrgAddress orgA) : base(orgA?.Header)
	{
	}

	protected static OrgAddress GetRepresentative(JobDeclaration jobDeclaration) => jobDeclaration != null
		&& (jobDeclaration.JE_DeclarantType == ESRepresentationTypeList.Codes._2Direct || jobDeclaration.JE_DeclarantType == ESRepresentationTypeList.Codes._5IndirectATC)
			? jobDeclaration.Representative ?? jobDeclaration.DeclarantAddress
			: null;
	
	readonly static ZString FixedStatusCode = "2";

	public ZString Status => FixedStatusCode;
}
