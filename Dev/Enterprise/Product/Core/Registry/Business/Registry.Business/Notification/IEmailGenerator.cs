using System;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public interface IEmailGenerator
	{
		Type DocSourceType { get; }

		BusinessObject CreatePreviewSample();
		EmailDef BuildEmail(string recipientEmailAddress, NotificationEmailTemplate template, BusinessObject dataSource);
	}
}
