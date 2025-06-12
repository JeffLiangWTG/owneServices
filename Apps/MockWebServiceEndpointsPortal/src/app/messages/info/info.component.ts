import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EndpointService } from "../../shared/endpoint.service";
import { ConfigService } from 'src/app/shared/config.service';
import * as md from '../../model/messaging-interface'
import { MessagesManager } from 'src/app/shared/messages.service';

@Component({
	templateUrl: './info.component.html',
	styleUrls: ['./info.component.css'],
	providers: [MessagesManager],
})

export class HelpComponent implements OnInit{
	mockEndpointsUrl: string = '';
	id: string = '';
	endpointInfo: md.MockEndpointInfo = {
		checkRunning: '',
		controllerName: '',
		optionalHtml: '',
		interfaceName: ''
	};
	infoStatus: md.AppSatus = {
		busy: false,
		message: ''
	}

	constructor(private route:ActivatedRoute, private messageManager:MessagesManager, configService: ConfigService, private enpointService: EndpointService){
		this.mockEndpointsUrl = configService.config.mockEndpointsUrl;
	}

	ngOnInit(): void {
		this.route.params.subscribe(params => {
			this.id = params['id'];
			if (this.id){
				this.messageManager.init(this.id, this.infoStatus);
				this.messageManager.loadInfo((data: any) => {
					this.endpointInfo = data;
				});
			}
		});
	}
}
