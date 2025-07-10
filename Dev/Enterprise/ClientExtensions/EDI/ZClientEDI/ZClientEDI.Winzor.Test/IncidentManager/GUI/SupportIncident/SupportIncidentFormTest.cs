using System;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.ZArchitecture.Environment;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Internal.Execution;
using WTG.RtfConverter.Html;

namespace ZClientEDI.Winzor.Test;

class SupportIncidentFormTest
{
	[Test]
	public void SupportIncidentNotificationHandlerShouldNotThrowExceptionWithNullFactory()
	{
		var mockHandler = new Mock<INotificationHandler>();
		NotificationHandler.SetHandler(mockHandler.Object);
		INotificationHandler supportIncidentNotificationHandler = new SupportIncidentForm.SupportIncidentNotificationHandler(null);
		Assert.DoesNotThrow(() => { supportIncidentNotificationHandler.ReportInformation("message", "caption"); });
		mockHandler.Verify(m => m.ReportInformation("message", "caption"), Times.Once, "ReportInformation should be called once");
	}
}
