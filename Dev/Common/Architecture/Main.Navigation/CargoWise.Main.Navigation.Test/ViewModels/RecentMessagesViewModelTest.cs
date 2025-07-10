using System.Collections.Generic;
using Enterprise.ZArchitecture.Core;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Test;

class RecentMessagesViewModelTest : TestCase
{
	Mock<IRecentMessagesRepository> mockRepository;
	protected override void SetUp()
	{
		base.SetUp();

		mockRepository = new Mock<IRecentMessagesRepository>();
	}

	public void TestTitle()
	{
		var uut = new RecentMessagesViewModel(mockRepository.Object);

		AssertType<ResourceString>(uut.Title);
		AssertEquals("Recent Messages", uut.Title);
	}

	public void TestRecentMessages()
	{
		var uut = new RecentMessagesViewModel(mockRepository.Object);

		AssertNotNull(uut.RecentMessages);
		AssertEquals(0, uut.RecentMessages.Count);
	}

	public void TestPropertyChanged_WhenRefresh()
	{
		var uut = new RecentMessagesViewModel(mockRepository.Object);

		var events = new List<string>();
		uut.PropertyChanged += (sender, args) => events.Add(args.PropertyName);

		AssertEquals(0, events.Count);

		uut.Refresh();

		AssertEquals(1, events.Count);
		AssertEquals("RecentMessages", events[0]);
	}

	public void TestRefresh()
	{
		_ = mockRepository
			.Setup(x => x.GetLatestMessages(It.IsAny<int>()))
			.Returns(new List<RecentMessage>
			{
				new() { Body = "Message 1" },
				new() { Body = "Message 2" }
			});

		var uut = new RecentMessagesViewModel(mockRepository.Object);

		AssertEquals(0, uut.RecentMessages.Count);

		uut.Refresh();

		AssertEquals(2, uut.RecentMessages.Count);
		AssertEquals("Message 1", uut.RecentMessages[0].Body);
		AssertEquals("Message 2", uut.RecentMessages[1].Body);
	}
}
