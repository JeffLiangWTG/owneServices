using System.Windows.Input;
using Moq;
using NUnit.Framework;

namespace CargoWise.Main.Navigation.Winzor.Test.Services;

public class RecentMessagesServiceTest
{
	Mock<IWinzorControl> _mockControl;
	Mock<IRecentMessagesRepository> _mockRepo;

	[SetUp]
	public void SetUp()
	{
		_mockControl = new Mock<IWinzorControl>();
		_mockControl.Setup(c => c.InvokeAsync(It.IsAny<Action>())).Callback<Action>(a => a());

		_mockRepo = new Mock<IRecentMessagesRepository>();
		_mockRepo.Setup(r => r.GetLatestMessages(It.IsAny<int>())).Returns(new List<RecentMessage>
		{
			new RecentMessage { SenderName = "Alice", JobType = "Work Item", JobCode = "WI0000033", Body = "Hello!", PostedTimeAgo = "2 minutes ago" },
			new RecentMessage { SenderName = "Bob", JobType = "Work Item", JobCode = "WI0000027", Body = "Meeting at 2 PM.", PostedTimeAgo = "1 day ago", SenderCompanyName = "Company Name" },
		});
	}

	[Test]
	public void RecentMessagesService_Title()
	{
		var uut = new RecentMessagesService(new RecentMessagesViewModel(_mockRepo.Object), _mockControl.Object);

		Assert.That(uut.Title, Is.EqualTo("Recent Messages"));
	}

	[Test]
	public void RecentMessagesService_RefreshLabel()
	{
		var uut = new RecentMessagesService(new RecentMessagesViewModel(_mockRepo.Object), _mockControl.Object);

		Assert.That(uut.RefreshLabel, Is.EqualTo("Refresh Recent Messages"));
	}

	[Test]
	public void RecentMessagesService_NoRecentMessagesLabel()
	{
		var uut = new RecentMessagesService(new RecentMessagesViewModel(_mockRepo.Object), _mockControl.Object);

		Assert.That(uut.NoRecentMessagesLabel, Is.EqualTo("No Recent Messages"));
	}

	[Test]
	public async Task RecentMessagesService_RefreshAsync()
	{
		var uut = new RecentMessagesService(new RecentMessagesViewModel(_mockRepo.Object), _mockControl.Object);

		Assert.That(uut.RecentMessages, Is.Empty);

		await uut.RefreshAsync();

		Assert.That(uut.RecentMessages, Has.Count.EqualTo(2));
	}

	[Test]
	public async Task RecentMessagesService_OpenJobCommandAsync()
	{
		var openedJobs = new List<object>();
		var mockCommand = new Mock<ICommand>();
		mockCommand.Setup(c => c.CanExecute(It.IsAny<object>())).Returns(true);
		mockCommand.Setup(c => c.Execute(It.IsAny<object>())).Callback<object>(openedJobs.Add);

		var viewModel = new RecentMessagesViewModel(_mockRepo.Object)
		{
			OpenJobCommand = mockCommand.Object
		};

		var uut = new RecentMessagesService(viewModel, _mockControl.Object);

		var guid = Guid.NewGuid();
		await uut.OpenJobAsync(guid);

		Assert.That(openedJobs, Has.Count.EqualTo(1));
		Assert.That(openedJobs, Contains.Item(guid));
	}
}
