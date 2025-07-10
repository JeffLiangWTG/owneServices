using System.Collections.ObjectModel;
using Bunit;
using CargoWise.Main.Navigation.Pages;
using Enterprise.Winzor.Architecture.Test;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Components;

sealed class RecentMessagesTest : BunitTestContext
{
	Mock<IRecentMessagesService> _mockService;

	[SetUp]
	public void SetUp()
	{
		_mockService = new Mock<IRecentMessagesService>();
		_mockService.SetupGet(service => service.Title).Returns("Recent Messages");
		_mockService.SetupGet(service => service.RecentMessages).Returns(() => new ObservableCollection<RecentMessage>
		{
			new RecentMessage { SenderName = "Alice", SenderCompanyName = "WISETECH", JobType = "Work Item", JobCode = "WI0000033", Body = "Hello!", PostedTimeAgo = "2 hours ago" },
			new RecentMessage { SenderName = "Bob", SenderCompanyName = "WISGLOSYD", JobType = "Incident", JobCode = "CS0000027", Body = "Meeting at 2 PM.", PostedTimeAgo = "7 mins ago" }
		});
	}

	[Test]
	public void RecentMessages_ShouldRender()
	{
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		var thumbnail = cut.Find(".cwn-recent-messages");
		Assert.That(thumbnail, Is.Not.Null);
	}

	[Test]
	public void RecentMessages_ShouldRenderMessages()
	{
		// Act
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		Assert.That(cut.FindAll(".cwn-recent-messages__item").Count, Is.EqualTo(2));

		Assert.That(cut.Markup, Does.Contain("Alice"));
		Assert.That(cut.Markup, Does.Contain("Bob"));

		Assert.That(cut.Markup, Does.Contain("Hello!"));
		Assert.That(cut.Markup, Does.Contain("Meeting at 2 PM."));
	}

	[Test]
	public void RecentMessages_ShouldShowEmptyState()
	{
		// Arrange
		var mockService = new Mock<IRecentMessagesService>();
		mockService.SetupGet(service => service.NoRecentMessagesLabel).Returns("No Recent Messages");
		mockService.SetupGet(service => service.RecentMessages).Returns([]);

		// Act
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, mockService.Object));

		// Assert
		Assert.That(cut.Markup, Does.Contain("No Recent Messages"));
	}

	[Test]
	public void RecentMessages_ShouldRenderSenderCompanyName()
	{
		// Arrange
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		// Assert
		Assert.That(cut.Markup, Does.Contain("WISETECH"));
		Assert.That(cut.Markup, Does.Contain("WISGLOSYD"));
	}

	[Test]
	public void RecentMessages_ShouldRenderLinks()
	{
		// Arrange
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		// Act
		var link = cut.FindAll(".cwn-recent-messages__link");

		// Assert
		Assert.That(link.Count, Is.EqualTo(2));
		Assert.That(link[0].TextContent, Is.EqualTo("WI0000033"));
		Assert.That(link[1].TextContent, Is.EqualTo("CS0000027"));
	}

	[Test]
	public void RecentMessages_ShouldRenderTimestamps()
	{
		// Arrange
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));

		// Assert
		Assert.That(cut.Markup, Does.Contain("2 hours ago"));
		Assert.That(cut.Markup, Does.Contain("7 mins ago"));
	}

	[Test]
	public void RecentMessages_RefreshButtonTriggersRefresh()
	{
		// Arrange
		_mockService.Setup(s => s.RefreshAsync()).Returns(Task.CompletedTask);

		var cut = RenderComponent<RecentMessages>(parameters =>
			parameters.Add(p => p.Service, _mockService.Object)
		);

		// Act
		cut.Find(".cwn-recent-messages__refresh-btn").Click();

		// Assert
		_mockService.Verify(s => s.RefreshAsync(), Times.Exactly(2));
	}

	[Test]
	public void RecentMessages_TriggersOnclick()
	{
		// Act
		var cut = RenderComponent<RecentMessages>(parameters => parameters.Add(p => p.Service, _mockService.Object));
		var divElement = cut.FindAll(".cwn-recent-messages__item").FirstOrDefault();
		divElement?.Click();
		// Assert
		_mockService.Verify(s => s.OpenJobAsync(It.IsAny<Guid>()), Times.Once());
	}
}
