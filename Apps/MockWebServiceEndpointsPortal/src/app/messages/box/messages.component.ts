import { Component, OnInit} from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { MessagesManager } from '../../shared/messages.service'
import {Message, Folder, Status, AppSatus} from '../../model/messaging-interface'
import { ConfigService } from 'src/app/shared/config.service';

@Component({
  templateUrl: './messages.component.html',
  providers: [MessagesManager]
})

export class MessagesComponent implements OnInit {
	title = '';
	id: string = '';
	subfolder: string = '';
	backSlash = '$BACKSLASH$';
	messages: Message[] = [];
	folders: Folder[] = [];
	uploading = false;
	messageContent: string = '';
	refresh = false;
	hasId = false;
	isVisibleFiles = true;
	searchText = "";
	currentMessageIndex: number = 0;
	mockEndpointsUrl: string = '';
	messagesCallback: Function = () => {
		this.load()
	};
	messageBoxStatus: AppSatus = {
		busy: false,
		message: ''
	}
	readonly MessageStatus = Status;
	constructor(private route:ActivatedRoute, private messageManager:MessagesManager, configService: ConfigService) {
		this.mockEndpointsUrl = configService.config.mockEndpointsUrl;
	}

	ngOnInit() {
		this.route.params.subscribe(params => {
			this.id = params['id'];
			if (this.id){
				this.hasId = true;
				this.subfolder = params['subfolder'];
				if (this.subfolder) {
					this.subfolder = this.subfolder.replaceAll(this.backSlash, "/");
					if (this.subfolder.endsWith("..")) {
						this.subfolder = this.subfolder.substring(0, this.subfolder.length - 3);
						var lastSlash = this.subfolder.indexOf("/");
						if (lastSlash > 0) {
							this.subfolder = this.subfolder.substring(0, lastSlash);
						} else {
							this.subfolder = '';
						}
					}
				}
				this.title = this.id + " " + "Message Box";
				this.messageManager.init(this.getPath(), this.messageBoxStatus);
				this.messageManager.loadSubFolders((result:any) => this.folders = result.folders, false);
				this.messageManager.load((result:any) => this.messages = result.messages, false);
				this.load();

				setInterval(() => {
					this.messageManager.load((result:any) => {
						var messagesJson = JSON.stringify(result.messages);
						if (messagesJson != JSON.stringify(this.messages)) {
							result.messages
								.forEach((message: Message) => {
									var existingMessage = this.messages.find(x => x.name === message.name);
									if (existingMessage != null) {
										existingMessage.created = message.created;
										existingMessage.modified = message.modified;
										existingMessage.size = message.size;
									} else {
										this.messages.unshift(message);
									}
								});
							for (var i = this.messages.length-1; i >= 0; i--) {
								var shouldDelete = result.messages.every((y: Message) => y.name != this.messages[i].name);
								if (shouldDelete) {
									this.messages.splice(i, 1);
								}
							}
						}
					}, true);
				}, 2000);
			}
		});
	}

	load(reload: boolean = false) {
		this.messageManager.loadSubFolders((result:any) => this.folders = result.folders, reload);
		this.messageManager.load((result:any) => this.messages = result.messages, reload);
	}

	remove(message: Message) {
		if (this.isViewOnly() && this.isVisibleFiles) {
			this.messageBoxStatus.message = "Cannot delete/move system message.";
			return;
		}
		this.close(message);
		this.messageManager.remove(message, () => {
			var i = this.messages.indexOf(message);
			this.messages.splice(i, 1);
		});
	}

	removeDir(folder: Folder) {
		if (this.isViewOnly() && this.isVisibleFiles) {
			this.messageBoxStatus.message = "Cannot delete/move system folder.";
			return;
		}

		this.messageManager.removeDir(folder, () => {
			var i = this.folders.indexOf(folder);
			this.folders.splice(i, 1);
		});
	}

	view(message: Message, editing: Boolean) {
		this.messageManager.view(message, (data: any) => {
			message.content = data.messageContent;
			message.status = editing ? Status.Editing : Status.Opening;
			var messageIndex = this.messages.indexOf(message);
			if (this.currentMessageIndex !== messageIndex)
			{
				var currentMessage = this.messages.at(this.currentMessageIndex);
				if (currentMessage) this.close(currentMessage);
			}

			if (this.messages.indexOf(message) !== -1) this.currentMessageIndex = messageIndex;
		});
	}

	edit(message: Message) {
		this.view(message, true);
	}

	saveEditing(message: Message) {
		message.status = Status.Saving;
		var uploadMessages: File[] = [];
		if (message.content){
			var file = new File([message.content], message.name);
			uploadMessages.push(file);
			this.messageManager.upload(uploadMessages, () => {
				this.close(message);
				this.load();
			});
		}
	}

	close(message: Message) {
		message.content = undefined;
		message.status = Status.Closed;
	}

	activeToggle() {
		this.messageManager.id = this.getPath();
		this.messageManager.load((result:any) => this.messages = result.messages, false);
	}

	getPath() {
		return this.id + (this.subfolder ? `/${this.subfolder}` : "").replaceAll("/", this.backSlash) + (this.isVisibleFiles ? "/active" : "/inactive");
	}

	active(message: Message, keepOriginal: string) {
		if (this.isViewOnly()) {
			this.messageManager.setInfo({ message: "Cannot delete/move system message." });
			return;
		}

		this.close(message);
		this.messageManager.toggleActivate(message, keepOriginal, () => {
			if (keepOriginal != 'true') {
				var i = this.messages.indexOf(message);
				this.messages.splice(i, 1);
			}
		});
	}

	inactive(message: Message, keepOriginal: string) {
		if (keepOriginal == 'false' && this.isViewOnly()) {
			this.messageManager.setInfo({ message: "Cannot delete/move system message." });
			return;
		}
		this.close(message);
		this.messageManager.toggleActivate(message, keepOriginal, () => {
			if (keepOriginal != 'true') {
				var i = this.messages.indexOf(message);
				this.messages.splice(i, 1);
			}
		});
	}

	isViewOnly() {
		return this.id == "Logging" || this.id == "Repository";
	}

	getFolderPath(folder: string) {
		return this.subfolder ? `${this.subfolder.replaceAll("/", this.backSlash)}${this.backSlash}${folder}` : folder;
	}
}